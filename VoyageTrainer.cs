using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new TrainerForm());
    }
}

internal sealed class Game : IDisposable
{
    private readonly Process process;
    private readonly IntPtr handle;
    private readonly long image;

    private const uint Build = 0x862A560D;
    private const uint ImageSize = 0x0C46F000;

    private const float CarryCapacityKg = 999_000f;

    private const long DataRva = 0x0B293000;
    private const long DataSize = 0x70CA1C;

    private const long WorldClass = 0x0B6C98C0;
    private const long InstanceClass = 0x0B69B990;
    private const long LocalPlayerClass = 0x0B6A4F40;
    private const long CharacterClass = 0x0B82A530;
    private const long BaseCharacterClass = 0x0B82AD88;
    private const long MovementClass = 0x0B82E568;
    private const long SurvivalClass = 0x0B8393F0;
    private const long WeightClass = 0x0B831070;
    private const long ModuleClass = 0x0B833528;
    private const long WeatherClass = 0x0B83C498;

    // Boat classes and resources.
    private const long BoatClass = 0x0B82A938;
    private const long BoatModuleSubsystemClass = 0x0B8345D8;

    private const int BoatThrottleOffset = 0x6B4;
    private const int BoatEnginePowerOffset = 0x6C4;

    private const byte ElectricityResource = 0;
    private const byte PetrolResource = 1;
    private const byte DieselResource = 16;

    private const int WaveStride = 0x40;
    private const long MinimapLocatorClass = 0x0B8339A8;
    private const long MinimapPropertyClass = 0x0B833D98;
    private const long MinimapComponentClass = 0x0B834230;
    private const long MinimapBoatClass = 0x0B82A938;


    private const long PawnClass = 0x0B6AE848;
    private const long VehiclePawnClass = 0x0B83C240;

    private const int VehicleDriverOffset = 0x430;

    public sealed record MapMarker(
        string Name,
        double X,
        double Y,
        bool Virtual);

    public sealed record MapSnapshot(
        double X,
        double Y,
        double Z,
        double Heading,
        MapMarker[] Markers);

    private volatile bool minimapEnabled;
    private volatile MapSnapshot? minimapSnapshot;

    private MapMarker[] cachedMapMarkers =
        Array.Empty<MapMarker>();

    private long cachedMapWorld;
    private long nextMapRefresh;

    public MapSnapshot? LatestMinimap => minimapSnapshot;

    public void SetMinimapEnabled(bool enabled)
    {
        minimapEnabled = enabled;
    }

    private void PublishMinimap(Context? context)
    {
        if (!minimapEnabled)
        {
            minimapSnapshot = null;
            nextMapRefresh = 0;
            return;
        }

        try
        {
            var c = context ?? current;

            if (c == null || !IsA(c.World, WorldClass))
            {
                minimapSnapshot = null;
                return;
            }

            long gi = Ptr(c.World + 0x228);

            if (!IsA(gi, InstanceClass))
            {
                minimapSnapshot = null;
                return;
            }

            long players = Ptr(gi + 0x38);

            if (!Pointer(players) || I32(gi + 0x40) != 1)
            {
                minimapSnapshot = null;
                return;
            }

            long lp = Ptr(players);
            long controller = Ptr(lp + 0x30);

            if (!IsA(lp, LocalPlayerClass) ||
                controller != c.Controller ||
                Ptr(controller + 0x348) != lp)
            {
                minimapSnapshot = null;
                return;
            }

            // Read the currently controlled actor.
            // This also supports taking control of the boat.
            long pawn = Ptr(controller + 0x2E8);

            // Track any valid possessed actor derived from APawn.
            if (!Pointer(pawn) ||
                !IsA(pawn, PawnClass) ||
                BoatObjectEntry(pawn) == 0)
            {
                minimapSnapshot = null;
                return;
            }

            long root = Ptr(pawn + 0x1B8);

            if (!Pointer(root))
            {
                minimapSnapshot = null;
                return;
            }

            // Quaternion followed by translation.
            byte[] transform = Read(root + 0x1F0, 56);

            double qx = BitConverter.ToDouble(transform, 0);
            double qy = BitConverter.ToDouble(transform, 8);
            double qz = BitConverter.ToDouble(transform, 16);
            double qw = BitConverter.ToDouble(transform, 24);

            double x = BitConverter.ToDouble(transform, 32);
            double y = BitConverter.ToDouble(transform, 40);
            double z = BitConverter.ToDouble(transform, 48);

            if (!ValidMapCoordinate(x, y, z))
            {
                minimapSnapshot = null;
                return;
            }

            double norm = Math.Sqrt(
                qx * qx + qy * qy +
                qz * qz + qw * qw);

            if (!double.IsFinite(norm) ||
                norm < 0.5 || norm > 1.5)
            {
                minimapSnapshot = null;
                return;
            }

            qx /= norm;
            qy /= norm;
            qz /= norm;
            qw /= norm;

            double yaw = Math.Atan2(
                2.0 * (qw * qz + qx * qy),
                1.0 - 2.0 * (qy * qy + qz * qz));

            double heading =
                (yaw * 180.0 / Math.PI + 360.0) % 360.0;

            long now = Environment.TickCount64;

            // Refresh the location cache every two seconds.
            if (cachedMapWorld != c.World ||
                now >= nextMapRefresh)
            {
                cachedMapMarkers = ReadMapMarkers(c.World);
                cachedMapWorld = c.World;
                nextMapRefresh = now + 2000;
            }

            // Verify that possession didn't change.
            if (Ptr(controller + 0x2E8) != pawn ||
                Ptr(pawn + 0x1B8) != root)
            {
                minimapSnapshot = null;
                return;
            }

            minimapSnapshot = new MapSnapshot(
                x, y, z, heading, cachedMapMarkers);
        }
        catch
        {
            // A failed map read must never interrupt the trainer.
            minimapSnapshot = null;
        }
    }

    private static bool ValidMapCoordinate(
        double x, double y, double z)
    {
        return double.IsFinite(x) &&
               double.IsFinite(y) &&
               double.IsFinite(z) &&
               Math.Abs(x) < 1e12 &&
               Math.Abs(y) < 1e12 &&
               Math.Abs(z) < 1e12;
    }

    private IEnumerable<long> MapEntries(
        long map, int stride)
    {
        long data = Ptr(map);

        int slots = I32(map + 0x08);
        int capacity = I32(map + 0x0C);
        int bitCount = I32(map + 0x28);

        if (slots == 0)
            yield break;

        if (!Pointer(data) ||
            slots < 0 ||
            slots > 8192 ||
            capacity < slots ||
            bitCount < slots)
        {
            yield break;
        }

        long bits = Ptr(map + 0x20);

        if (bits == 0)
        {
            if (bitCount > 128)
                yield break;

            bits = map + 0x10;
        }

        if (!Pointer(bits))
            yield break;

        byte[] flags = Read(
            bits,
            ((slots + 31) / 32) * 4);

        for (int i = 0; i < slots; i++)
        {
            uint allocated = BitConverter.ToUInt32(
                flags, (i / 32) * 4);

            if ((allocated & (1u << (i % 32))) == 0)
                continue;

            yield return data + i * (long)stride;
        }
    }

    private long FindMinimapSubsystem(long world)
    {
        long cls = Ptr(image + MinimapLocatorClass);

        if (!Pointer(cls))
            return 0;

        foreach (long entry in MapEntries(
            world + 0x9F0, 0x18))
        {
            if (Ptr(entry) != cls)
                continue;

            long obj = Ptr(entry + 0x08);

            if (IsA(obj, MinimapLocatorClass) &&
                Ptr(obj + 0x20) == world)
            {
                return obj;
            }
        }

        return 0;
    }

    private long ResolveMapWeakPointer(long address)
    {
        int index = I32(address);
        int serial = I32(address + 4);

        if (index < 0 || serial == 0)
            return 0;

        int count = I32(image + 0x0B647748);

        if (index >= count)
            return 0;

        long chunks = Ptr(image + 0x0B647740);

        if (!Pointer(chunks))
            return 0;

        long chunk = Ptr(
            chunks + (index >> 16) * 8L);

        if (!Pointer(chunk))
            return 0;

        long entry =
            chunk + (index & 0xFFFF) * 24L;

        if (I32(entry + 0x10) != serial)
            return 0;

        if ((U32(entry + 4) & 0x10200000u) != 0)
            return 0;

        return Ptr(entry + 8);
    }

    private string ReadMapName(long entry)
    {
        try
        {
            long data = Ptr(entry);
            int length = I32(entry + 8);
            int capacity = I32(entry + 12);

            if (!Pointer(data) ||
                length <= 1 ||
                length > 256 ||
                capacity < length)
            {
                return "Location";
            }

            return System.Text.Encoding.Unicode
                .GetString(Read(data, length * 2))
                .TrimEnd('\0');
        }
        catch
        {
            return "Location";
        }
    }

    private MapMarker[] ReadMapMarkers(long world)
    {
        var result = new List<MapMarker>();

        long subsystem = FindMinimapSubsystem(world);

        if (!Pointer(subsystem))
            return result.ToArray();

        // Registered physical locator components.
        foreach (long entry in MapEntries(
            subsystem + 0x2B0, 0x20))
        {
            try
            {
                long component = ResolveMapWeakPointer(
                    entry + 0x10);

                if (!IsA(component, MinimapComponentClass))
                    continue;

                double x = Double(component + 0x210);
                double y = Double(component + 0x218);
                double z = Double(component + 0x220);

                if (!ValidMapCoordinate(x, y, z))
                    continue;

                result.Add(new MapMarker(
                    ReadMapName(entry),
                    x, y, false));
            }
            catch
            {
                // Skip a locator that was just unloaded.
            }
        }

        // Virtual locators, including locations not streamed in.
        foreach (long entry in MapEntries(
            subsystem + 0x300, 0x118))
        {
            try
            {
                long value = entry + 0x10;

                long position = value + 0x70;

                if (Byte(value + 0x60) != 0)
                {
                    long property = Ptr(value + 0xC8);

                    if (property != 0)
                    {
                        if (!IsA(property, MinimapPropertyClass))
                            continue;

                        position = property + 0x30;
                    }
                }

                double x = Double(position);
                double y = Double(position + 8);
                double z = Double(position + 16);

                if (!ValidMapCoordinate(x, y, z))
                    continue;

                result.Add(new MapMarker(
                    ReadMapName(entry),
                    x, y, true));
            }
            catch
            {
                // Skip unavailable virtual locations.
            }
        }

        return result.ToArray();
    }
    private record Context(
        long World,
        long Controller,
        long Pawn,
        long Movement,
        long Survival,
        long Weight,
        long Settings,
        long Camera);

    private record Field(
        int Owner,
        int Offset,
        float Factor,
        float Absolute = 0);

    private record Saved(
        Context Context,
        long Address,
        float Original,
        Field Field);

    private sealed record Toggle(
        string Name,
        Field[] Fields);

    private sealed record CodePatch(
        string Name,
        long Rva,
        byte[] Original,
        byte[] Enabled);

    private static readonly Dictionary<int, Toggle> Toggles = new()
    {
        [5] = new("Carry capacity 999 tonnes", new[]
        {
            new Field(5, 0x9E4, 0, CarryCapacityKg),
            new Field(2, 0x4A8, 0, CarryCapacityKg),
            new Field(2, 0x4F0, 0, CarryCapacityKg)
        }),

        [6] = new("Walking speed", new[]
        {
            new Field(0, 0x288, 2),
            new Field(0, 0x28C, 2)
        }),

        [7] = new("Swimming speed", new[]
        {
            new Field(0, 0x290, 2)
        }),

        [8] = new("Jump height", new[]
        {
            new Field(0, 0x1B0, 2)
        }),

        [9] = new("Low gravity", new[]
        {
            new Field(0, 0x1A8, 0.25f)
        }),

        [10] = new("Game time speed", new[]
        {
            new Field(3, 0x3E0, 2)
        }),

        [11] = new("Camera FOV", new[]
        {
            new Field(4, 0x2C0, 0, 100)
        }),

        [13] = new("Clear Skies", new[]
        {
            new Field(6, 0xDC, 0, 0),
            new Field(6, 0xE4, 0, 0),
            new Field(6, 0xE8, 0, 0),
            new Field(6, 0x24C, 0, 0),
            new Field(6, 0x254, 0, 0),
            new Field(6, 0x258, 0, 0)
        }),

        [14] = new("Always Daytime", new[]
        {
            new Field(6, 0x220, 0, 0)
        }),

        [15] = new("Calm Ocean", new[]
        {
            new Field(6, 0x290, 0, 0),
            new Field(6, 0x294, 0, 0),
            new Field(6, 0x2D0, 0, 0),
            new Field(6, 0x2D4, 0, 0)
        }),

        [16] = new("Disable Lightning", new[]
        {
            new Field(6, 0xE0, 0, 0),
            new Field(6, 0x250, 0, 0)
        })
    };

    private static readonly CodePatch[] FreeCraftPatches =
    {
        new(
            "Material deduction",
            0x55D727A,
            new byte[] { 0xE8, 0x31, 0x0B, 0x0E, 0x00 },
            new byte[] { 0x44, 0x89, 0xF0, 0x90, 0x90 }),

        new(
            "Craft material availability",
            0x55D6FC8,
            new byte[] { 0x7C, 0x25 },
            new byte[] { 0x90, 0x90 }),

        new(
            "Grouped recipe availability",
            0x55D5F7E,
            new byte[] { 0x7C, 0x5C },
            new byte[] { 0x90, 0x90 }),

        new(
            "Recipe availability",
            0x55D5910,
            new byte[] { 0x7C, 0x1E },
            new byte[] { 0x90, 0x90 }),

        new(
            "Fabricator recipe availability",
            0x550154A,
            new byte[] { 0x7D, 0x1F },
            new byte[] { 0xEB, 0x1F }),

        new(
            "Fabricator queue availability",
            0x550195E,
            new byte[] { 0x7C, 0x26 },
            new byte[] { 0x90, 0x90 }),

        new(
            "Fabricator material deduction",
            0x5522413,
            new byte[] { 0xE8, 0x88, 0xD3, 0xFF, 0xFF },
            new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90 }),
        new(
            "Instant fabrication duration",
            0x552244D,
            new byte[]
            {
                0xF3, 0x0F, 0x59, 0x81,
                0x68, 0x01, 0x00, 0x00
            },
            new byte[]
            {
                0xF3, 0x0F, 0x10, 0x05,
                0x6B, 0xBE, 0x66, 0x03
            }),

        new(
            "Instant fabrication countdown A",
            0x5514F18,
            new byte[] { 0xF3, 0x41, 0x0F, 0x5C, 0xC7 },
            new byte[] { 0x0F, 0x57, 0xC0, 0x90, 0x90 }),

        new(
            "Instant fabrication countdown B",
            0x5514F74,
            new byte[] { 0xF3, 0x41, 0x0F, 0x5C, 0xD7 },
            new byte[] { 0x0F, 0x57, 0xD2, 0x90, 0x90 })
    };


    private static CodePatch WeatherPatch(
    string name,
    long rva,
    string original,
    string replacement)
    {
        return new CodePatch(
            name,
            rva,
            Convert.FromHexString(original),
            Convert.FromHexString(replacement));
    }

    private static readonly Dictionary<int, CodePatch[]> WeatherPatches = new()
    {
        // Clear Skies
        [13] = new[]
        {
        WeatherPatch(
            "Default cloud coverage",
            0x56F0305,
            "48C7461C0000803E",
            "48C7461C00000000"),

        WeatherPatch(
            "Precipitation calculation",
            0x56F0FE9,
            "F30F5EC1",
            "0F57C090"),

        WeatherPatch(
            "Cloud coverage calculation",
            0x56F1013,
            "F30F5EC1",
            "0F57C090"),

        WeatherPatch(
            "Fog density calculation",
            0x56F1041,
            "F30F5EC1",
            "0F57C090"),

        WeatherPatch(
            "World state precipitation",
            0x53322A7,
            "8B4334",
            "31C090"),

        WeatherPatch(
            "World state cloud coverage",
            0x53322B3,
            "8B433C",
            "31C090"),

        WeatherPatch(
            "World state fog density",
            0x53322B9,
            "8B4340",
            "31C090")
    },

        // Always Daytime
        [14] = new[]
        {
        WeatherPatch(
            "Reported time of day",
            0x5332267,

            "488BD8488D5424208B4808894F084C8D4358" +
            "8B480C894F0C8B4810894F108B4814894F14" +
            "8B4818894F188B4828894F288B482C894F2C" +
            "488D4F588B4030894730",

            "4893488D542420C74708000068414C8D4358" +
            "8B4B0C894F0C8B4B10894F108B4B14894F14" +
            "8B4B18894F188B4B28894F288B4B2C894F2C" +
            "488D4F588B4330894730")
    },

        // Calm Ocean
        [15] = new[]
        {
        WeatherPatch(
            "Minimum wave amplitude",
            0x56F0E45,
            "F30F1093B0000000",
            "F30F101573D44903"),

        WeatherPatch(
            "Maximum wave amplitude",
            0x56F0E52,
            "F30F109BB4000000",
            "F30F101DAAA24C03"),

        WeatherPatch(
            "Minimum amplitude multiplier",
            0x56F0EB1,
            "F30F5916",
            "90909090"),

        WeatherPatch(
            "Maximum amplitude multiplier",
            0x56F0EBA,
            "F30F591E",
            "90909090"),

        WeatherPatch(
            "Small wave steepness",
            0x56F0EC3,
            "F30F594604",
            "0F57C09090"),

        WeatherPatch(
            "Large wave steepness",
            0x56F0ECD,
            "F30F594E04",
            "0F57C99090")
    },

        // Disable Lightning
        [16] = new[]
        {
        WeatherPatch(
            "Default thunder intensity",
            0x56F02FA,
            "C746180000803F",
            "C7461800000000"),

        WeatherPatch(
            "Thunder intensity calculation",
            0x56F1069,
            "F30F5EC1",
            "0F57C090"),

        WeatherPatch(
            "World state thunder",
            0x53322AD,
            "8B4338",
            "31C090")
    }
    };

    private readonly Dictionary<int, List<CodePatch>>
        appliedWeather = new();


    private static readonly CodePatch HumanSeedDecayPatch = new(
        "Human seed decay",
        0x54266C1,
        new byte[] { 0xF3, 0x0F, 0x5C, 0xCE },
        new byte[] { 0x90, 0x90, 0x90, 0x90 });

        private bool humanSeedFrozen;


    private readonly Dictionary<int, List<Saved>> active = new();

    private readonly ConcurrentDictionary<int, float> multipliers =
        new()
        {
            [6] = 2f,
            [7] = 2f,
            [8] = 2f,
            [9] = 0.5f,
            [10] = 2f,
            [11] = 100f
        };

    private readonly List<CodePatch> appliedCraft = new();

    private readonly ConcurrentQueue<int> requests = new();

    private readonly Stopwatch recoveryTimer =
        Stopwatch.StartNew();

    private readonly Stopwatch deepRecoveryTimer =
        Stopwatch.StartNew();

    private readonly Stopwatch boatResourceTimer =
        Stopwatch.StartNew();

    private readonly Stopwatch boatThrottleTimer =
        Stopwatch.StartNew();

    private bool unlimitedHealth;
    private bool unlimitedBattery;

    private bool unlimitedBoatFuel;
    private bool unlimitedBoatElectricity;
    private bool maximumBoatThrottle;

    private volatile bool stop;

    private Context? current;
    private bool paused;

    private string? healthError;
    private string? batteryError;
    private string? validationError;
    private string? boatError;

    private long freezeWrites;

    private long weatherObject;
    private long weatherWorld;

    private long lastBoat;
    private long lastBoatWorld;
    private long lastBoatPlayer;
    private int lastBoatSerial;

    private long throttleBoat;

    public Game()
    {
        var matches = Process.GetProcessesByName(
            "VoyageSteam-Win64-Shipping");

        if (matches.Length != 1)
        {
            foreach (var p in matches)
                p.Dispose();

            throw new InvalidOperationException(
                "Start exactly one game instance " +
                "and load a single-player save.");
        }

        process = matches[0];

        image = process.MainModule?.BaseAddress.ToInt64()
            ?? throw new InvalidOperationException(
                "Cannot obtain game module base.");

        handle = OpenProcess(
            0x400 | 0x10 | 0x20 | 0x8,
            false,
            process.Id);

        if (handle == IntPtr.Zero)
        {
            process.Dispose();

            throw new InvalidOperationException(
                $"OpenProcess failed: " +
                $"{Marshal.GetLastWin32Error()}.");
        }

        try
        {
            long pe = image + I32(image + 0x3C);

            if (U32(pe) != 0x4550 ||
                U32(pe + 8) != Build ||
                U32(pe + 24 + 56) != ImageSize)
            {
                throw new InvalidOperationException(
                    "Game build does not match the supplied " +
                    "executable. No memory will be changed.");
            }
        }
        catch
        {
            CloseHandle(handle);
            process.Dispose();
            throw;
        }
    }

    public void Request(int id)
        => requests.Enqueue(id);

    public void Stop()
        => stop = true;

    public void SetMultiplier(int id, float value)
    {
        if (!float.IsFinite(value))
            return;

        switch (id)
        {
            case 6:
            case 7:
            case 8:
                multipliers[id] =
                    Math.Clamp(value, 0.5f, 5f);
                break;

            case 9:
                multipliers[id] =
                    Math.Clamp(value, 0.1f, 2f);
                break;

            case 10:
                multipliers[id] =
                    Math.Clamp(value, 0.5f, 3f);
                break;

            case 11:
                multipliers[id] =
                    Math.Clamp(value, 60f, 130f);
                break;
        }
    }

    // =========================================================
    // MAIN WORKER
    // =========================================================

    public void Run(System.Action<string, bool[]> status)
    {
        Console.WriteLine(
            $"Voyage trainer | PID {process.Id} | " +
            $"Module 0x{image:X} | Build {Build:X8}");

        var tickTimer = Stopwatch.StartNew();
        var displayTimer = Stopwatch.StartNew();
        var mapTimer = Stopwatch.StartNew();

        bool highResolution = timeBeginPeriod(1) == 0;

        try
        {
            TryResolvePlayer(true);

            while (!stop && !process.HasExited)
            {
                while (!stop &&
                       requests.TryDequeue(out int id))
                {
                    try
                    {
                        switch (id)
                        {
                            case -1:
                                if (Live() == null)
                                    TryResolvePlayer(true);
                                else
                                    PrintStatus();
                                break;

                            case -2:
                                PrintStatus();
                                break;

                            case -3:
                                RestoreAll();

                                Console.WriteLine(
                                    "All trainer options disabled.");
                                break;

                            default:
                                Action(id);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Command {id}: {ex.Message}");
                    }
                }

                bool activeFeatures =
                    unlimitedHealth ||
                    unlimitedBattery ||
                    unlimitedBoatFuel ||
                    unlimitedBoatElectricity ||
                    maximumBoatThrottle ||
                    active.Count != 0;

                int interval = activeFeatures ? 5 : 250;

                if (tickTimer.ElapsedMilliseconds >= interval)
                {
                    tickTimer.Restart();

                    Tick();
                }

                if (displayTimer.ElapsedMilliseconds >= 400)
                {
                    var c = Live();

                    string info = c == null
                        ? "Waiting for verified player..."
                        : $"Player 0x{c.Pawn:X}";

                    if (c != null)
                    {
                        try
                        {
                            var h = Attribute(c, 8);

                            info +=
                                $" | Health {h.Value:0.##}/" +
                                $"{h.Max:0.##}";
                        }
                        catch
                        {
                            info += " | Health unavailable";
                        }

                        try
                        {
                            var b = Battery(c);

                            info +=
                                $" | Battery {b.Value:0.##}/" +
                                $"{b.Max:0.##}";
                        }
                        catch
                        {
                            info += " | Battery unavailable";
                        }

                        try
                        {
                            info +=
                                $" | Carry {EffectiveCarry(c):N0} kg";
                        }
                        catch
                        {
                            info += " | Carry unavailable";
                        }

                        try
                        {
                            long boat = GetBoat(c);

                            if (boat != 0)
                            {
                                info +=
                                    $" | Boat throttle " +
                                    $"{Float(boat + BoatThrottleOffset):P0}";
                            }
                        }
                        catch
                        {
                        }
                    }
                    bool[] enabled = new bool[22];

                    enabled[3] = unlimitedHealth;
                    enabled[4] = unlimitedBattery;

                    foreach (int key in active.Keys)
                        enabled[key] = true;

                    enabled[12] =
                        appliedCraft.Count ==
                        FreeCraftPatches.Length;

                    enabled[18] = unlimitedBoatFuel;
                    enabled[19] = unlimitedBoatElectricity;
                    enabled[20] = maximumBoatThrottle;
                    enabled[21] = humanSeedFrozen;

                    // Weather feature statuses
                    foreach (var (id, patches) in appliedWeather)
                    {
                        enabled[id] =
                            patches.Count == WeatherPatches[id].Length;
                    }
                    if (paused)
                        info += " | Writes paused";

                    if (boatError != null)
                        info += $" | Boat: {boatError}";

                    status(info, enabled);

                    displayTimer.Restart();
                }


                if (mapTimer.ElapsedMilliseconds >= 50)
                {
                    mapTimer.Restart();

                    PublishMinimap(current);
                }

                Thread.Sleep(1);
            }
        }
        finally
        {
            try
            {
                RestoreAll();
            }
            finally
            {
                if (highResolution)
                    timeEndPeriod(1);
            }
        }
    }

    private void Action(int id)
    {
        try
        {
            if (id >= 13 && id <= 16)
            {
                ToggleWeather(id);
                return;
            }
            if (id == 21)
            {
                if (!humanSeedFrozen && Live() == null)
                {
                    throw new InvalidOperationException(
                        "Load a single-player save first.");
                }

                ToggleHumanSeedDecay();
                return;
            }

            if (id >= 17 && id <= 20)
            {
                BoatAction(id);
                return;
            }

            if (id == 12)
            {
                if (appliedCraft.Count == 0 && Live() == null)
                {
                    throw new InvalidOperationException(
                        "Load a single-player save first.");
                }

                ToggleFreeCraft();
                return;
            }

            if (id == 3 && unlimitedHealth)
            {
                unlimitedHealth = false;
                healthError = null;

                Console.WriteLine("Unlimited health: OFF");
                return;
            }

            if (id == 4 && unlimitedBattery)
            {
                unlimitedBattery = false;
                batteryError = null;

                Console.WriteLine("Unlimited battery: OFF");
                return;
            }

            if (active.Remove(id, out var old))
            {
                Restore(old);

                Console.WriteLine(
                    $"{Toggles[id].Name}: OFF");

                return;
            }

            var c = Live()
                ?? throw new InvalidOperationException(
                    "No verified local player.");

            switch (id)
            {
                case 1:
                    Refill(c, 8);
                    Console.WriteLine("Health refilled.");
                    return;

                case 2:
                    RefillBattery(c);

                    Console.WriteLine(
                        "Battery refill requested successfully.");

                    return;

                case 3:
                    Refill(c, 8);

                    unlimitedHealth = true;

                    Console.WriteLine(
                        "Unlimited health: ON");

                    return;

                case 4:
                    RefillBattery(c);

                    unlimitedBattery = true;
                    batteryError = null;

                    Console.WriteLine(
                        "Unlimited battery: ON");

                    return;
            }

            if (!Toggles.TryGetValue(id, out var toggle))
                return;

            if (id >= 13 && id <= 16)
            {
                ResolveWeather(c);

                c = Live()
                    ?? throw new InvalidOperationException(
                        "Player changed during weather resolution.");

                ValidWeather(c);
            }

            var fields = new List<Field>(toggle.Fields);

            if (id == 15)
            {
                var waves = WaveArray(c);

                for (int i = 0; i < waves.Count; i++)
                {
                    fields.Add(new Field(
                        7,
                        i * WaveStride + 0x20,
                        0,
                        0));

                    fields.Add(new Field(
                        7,
                        i * WaveStride + 0x24,
                        0,
                        0));
                }
            }

            var saved = new List<Saved>();

            foreach (var field in fields)
            {
                long address = Address(c, field);
                float value = Float(address);

                if (!Reasonable(field, value))
                {
                    throw new InvalidOperationException(
                        $"Unexpected value {value} " +
                        $"at 0x{address:X}.");
                }

                saved.Add(
                    new Saved(c, address, value, field));
            }

            try
            {
                foreach (var s in saved)
                    Apply(id, s, c);
            }
            catch
            {
                Restore(saved);
                throw;
            }

            active.Add(id, saved);

            Console.WriteLine(
                $"{toggle.Name}: ON");

            if (id == 5)
                PrintCarryStatus(c);
        }
        catch (Exception ex)
        {
            string message =
                $"Feature {id} failed: {ex.Message}";

            Console.WriteLine(message);
            Debug.WriteLine(message);
        }
    }

    private void Tick()
    {
        var c = Live();

        TickBoat(c);

        if (c == null)
        {
            if (!paused)
            {
                Console.WriteLine(
                    "Player temporarily unavailable: " +
                    validationError +
                    ". Writes paused; enabled toggles retained.");

                recoveryTimer.Restart();
                deepRecoveryTimer.Restart();

                paused = true;
            }

            if (recoveryTimer.ElapsedMilliseconds >= 3000)
            {
                recoveryTimer.Restart();

                bool deep =
                    deepRecoveryTimer.ElapsedMilliseconds >= 30000;

                if (deep)
                    deepRecoveryTimer.Restart();

                TryResolvePlayer(deep);
            }

            return;
        }

        if (paused)
        {
            Console.WriteLine(
                "Player verified again. Resuming toggles.");

            paused = false;
        }

        recoveryTimer.Restart();
        deepRecoveryTimer.Restart();

        try
        {
            if (unlimitedHealth)
                Refill(c, 8);
        }
        catch (Exception ex)
        {
            if (healthError != ex.Message)
                Console.WriteLine($"Health retrying: {ex.Message}");

            healthError = ex.Message;
        }

        try
        {
            if (unlimitedBattery)
                RefillBattery(c);
        }
        catch (Exception ex)
        {
            if (batteryError != ex.Message)
                Console.WriteLine($"Battery retrying: {ex.Message}");

            batteryError = ex.Message;
        }

        foreach (var (id, saved) in active.ToArray())
        {
            try
            {
                // Do not discard movement options merely
                // because a component is temporarily unavailable.
                bool unavailable = saved.Any(s =>
                    (s.Field.Owner == 0 && c.Movement == 0) ||
                    (s.Field.Owner == 2 && c.Weight == 0) ||
                    (s.Field.Owner == 3 && c.Settings == 0) ||
                    (s.Field.Owner == 4 && c.Camera == 0));

                if (unavailable)
                    continue;

                foreach (var s in saved)
                    Apply(id, s, c);
            }
            catch (Exception ex)
            {
                active.Remove(id);

                Restore(saved);

                Console.WriteLine(
                    $"{Toggles[id].Name} disabled: {ex.Message}");
            }
        }
    }

    // =========================================================
    // PLAYER RESOLUTION - ON FOOT OR CONTROLLING BOAT
    // =========================================================

    private bool IsCharacter(long pawn)
    {
        return Pointer(pawn) &&
            (IsA(pawn, CharacterClass) ||
             IsA(pawn, BaseCharacterClass));
    }

    private long GetCharacterOrDriver(long controller)
    {
        if (!Pointer(controller))
            return 0;

        long controlled = Ptr(controller + 0x2E8);

        if (!Pointer(controlled))
            return 0;

        // Normal player possession.
        if (IsCharacter(controlled))
            return controlled;

        // Support all vehicles inheriting from the
        // shared vehicle base class.
        bool isVehicle =
            IsA(controlled, VehiclePawnClass) ||
            IsA(controlled, BoatClass);

        if (!isVehicle)
            return 0;

        // Verify that the vehicle is still a live object.
        if (BoatObjectEntry(controlled) == 0)
            return 0;

        long driver = Ptr(
            controlled + VehicleDriverOffset);

        if (!IsCharacter(driver) ||
            BoatObjectEntry(driver) == 0)
        {
            return 0;
        }

        // Ensure possession did not change during resolution.
        if (Ptr(controller + 0x2E8) != controlled ||
            Ptr(controlled + VehicleDriverOffset) != driver)
        {
            return 0;
        }

        return driver;
    }

    private Context? Live()
    {
        if (current == null)
        {
            validationError = "World not resolved";
            return null;
        }

        try
        {
            var c = ValidateWorld(
                current.World,
                out validationError);

            if (c == null)
                return null;

            current = c;

            return c;
        }
        catch (Exception ex)
        {
            validationError = ex.Message;
            return null;
        }
    }

    private Context? ValidateWorld(long w)
        => ValidateWorld(w, out _);

    private Context? ValidateWorld(
        long w,
        out string? reason)
    {
        reason = "World class";

        try
        {
            if (!IsA(w, WorldClass))
                return null;

            reason = "Game instance";

            long gi = Ptr(w + 0x228);

            if (!IsA(gi, InstanceClass))
                return null;

            reason = "Local player list";

            long players = Ptr(gi + 0x38);

            int count = I32(gi + 0x40);
            int capacity = I32(gi + 0x44);

            if (!Pointer(players) ||
                count != 1 ||
                capacity < count ||
                capacity > 16)
            {
                return null;
            }

            long lp = Ptr(players);

            if (!IsA(lp, LocalPlayerClass))
                return null;

            reason = "Player controller";

            long pc = Ptr(lp + 0x30);

            if (!Pointer(pc) ||
                Ptr(pc + 0x348) != lp)
            {
                return null;
            }

            reason = "Controlled pawn or vehicle driver";

            long pawn = GetCharacterOrDriver(pc);

            if (!Pointer(pawn))
                return null;

            reason = "Survival component";

            long survival = Ptr(pawn + 0x698);

            if (!IsA(survival, SurvivalClass))
                return null;

            long movement = OptionalComponent(
                pawn + 0x330,
                MovementClass);

            long weight = OptionalComponent(
                pawn + 0x6B8,
                WeightClass);

            long level = OptionalPointer(w + 0x30);

            long settings = Pointer(level)
                ? OptionalPointer(level + 0x2B8)
                : 0;

            long camera = OptionalPointer(pc + 0x360);

            var c = new Context(
                w,
                pc,
                pawn,
                movement,
                survival,
                weight,
                settings,
                camera);

            if (GetCharacterOrDriver(pc) != pawn)
            {
                reason = "Player possession changed";
                return null;
            }

            reason = null;

            return c;
        }
        catch (Exception ex)
        {
            reason = $"{reason}: {ex.Message}";
            return null;
        }
    }

    private void TryResolvePlayer(bool deep)
    {
        if (stop || process.HasExited)
            return;

        try
        {
            Resolve(deep);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"Player recovery failed: {ex.Message}");
        }
    }

    private void Resolve(bool deep = true)
    {
        if (Live() != null)
            return;

        long cls = Ptr(image + WorldClass);

        if (!Pointer(cls))
            return;

        Console.WriteLine(
            "Searching module data for the local world...");

        var matches =
            new Dictionary<long, (Context Context, long Slot)>();

        var seen = new HashSet<long>();

        for (long pos = image + DataRva;
             !stop && pos < image + DataRva + DataSize;
             pos += 65536)
        {
            int length = (int)Math.Min(
                65536,
                image + DataRva + DataSize - pos);

            var bytes = TryRead(pos, length);

            if (bytes == null)
                continue;

            for (int i = 0;
                 i + 8 <= bytes.Length;
                 i += 8)
            {
                long p = BitConverter.ToInt64(bytes, i);

                if (!Pointer(p) || !seen.Add(p))
                    continue;

                var type = TryRead(p + 0x10, 8);

                if (type == null ||
                    BitConverter.ToInt64(type) != cls)
                {
                    continue;
                }

                var c = ValidateWorld(p);

                if (c != null)
                    matches[p] = (c, pos + i);
            }
        }

        if (matches.Count == 0 && deep)
        {
            Console.WriteLine(
                "Searching readable private memory...");

            var timer = Stopwatch.StartNew();

            for (long p = 0x10000;
                 !stop &&
                 p < 0x00007FFFFFFFFFFF &&
                 timer.Elapsed.TotalSeconds < 30;)
            {
                if (VirtualQueryEx(
                    handle,
                    new IntPtr(p),
                    out var r,
                    (UIntPtr)48) == UIntPtr.Zero)
                {
                    break;
                }

                long start = r.BaseAddress.ToInt64();

                long size =
                    (long)r.RegionSize.ToUInt64();

                if (size <= 0 || start + size <= p)
                    break;

                p = start + size;

                if (r.State != 0x1000 ||
                    r.Type != 0x20000 ||
                    (r.Protect & 0x100) != 0 ||
                    (r.Protect & 0xFF) is not
                        (0x02 or 0x04 or 0x08 or
                         0x20 or 0x40 or 0x80))
                {
                    continue;
                }

                for (long off = 0;
                     !stop &&
                     off < size &&
                     timer.Elapsed.TotalSeconds < 30;
                     off += 1048576)
                {
                    var bytes = TryRead(
                        start + off,
                        (int)Math.Min(1048576, size - off));

                    if (bytes == null)
                        continue;

                    for (int i = 0;
                         i + 8 <= bytes.Length;
                         i += 8)
                    {
                        if (BitConverter.ToInt64(bytes, i) != cls)
                            continue;

                        long candidate =
                            start + off + i - 0x10;

                        var c = ValidateWorld(candidate);

                        if (c != null)
                            matches[candidate] = (c, 0);
                    }
                }
            }
        }

        if (stop)
            return;

        if (matches.Count != 1)
        {
            if (matches.Count > 1)
                current = null;

            Console.WriteLine(
                $"Found {matches.Count} verified worlds. " +
                "Waiting for player.");

            return;
        }

        current = matches.Values.Single().Context;

        Console.WriteLine(
            $"Player verified: 0x{current.Pawn:X}");

        PrintStatus();
    }

    // =========================================================
    // BOAT DETECTION
    // =========================================================

    private long BoatObjectEntry(long obj)
    {
        if (!Pointer(obj))
            return 0;

        int index = I32(obj + 0x0C);

        int count = I32(image + 0x0B647748);

        if (index < 0 || index >= count)
            return 0;

        long chunks = Ptr(image + 0x0B647740);

        if (!Pointer(chunks))
            return 0;

        long chunk = Ptr(
            chunks + (index >> 16) * 8L);

        if (!Pointer(chunk))
            return 0;

        long entry =
            chunk + (index & 0xFFFF) * 24L;

        if (Ptr(entry + 8) != obj)
            return 0;

        if ((U32(entry + 4) & 0x10200000u) != 0)
            return 0;

        return entry;
    }

    private long GetBoat(Context c, bool helmOnly = false)
    {
        long controlled = Ptr(c.Controller + 0x2E8);

        if (Pointer(controlled) &&
            IsA(controlled, BoatClass))
        {
            if (GetCharacterOrDriver(c.Controller) != c.Pawn)
                return 0;

            long entry = BoatObjectEntry(controlled);

            if (entry == 0)
                return 0;

            lastBoat = controlled;
            lastBoatWorld = c.World;
            lastBoatPlayer = c.Pawn;

            lastBoatSerial = I32(entry + 0x10);

            return controlled;
        }

        if (helmOnly)
            return 0;

        // Reuse the previously verified boat while on foot.
        if (controlled != c.Pawn ||
            lastBoatWorld != c.World ||
            lastBoatPlayer != c.Pawn ||
            lastBoatSerial <= 0 ||
            !Pointer(lastBoat))
        {
            return 0;
        }

        long cachedEntry = BoatObjectEntry(lastBoat);

        if (cachedEntry == 0 ||
            I32(cachedEntry + 0x10) != lastBoatSerial ||
            !IsA(lastBoat, BoatClass))
        {
            lastBoat = 0;
            return 0;
        }

        return lastBoat;
    }

    private List<long> BoatSparseEntries(
        long map,
        int stride,
        int limit = 65536)
    {
        var result = new List<long>();

        long data = Ptr(map);

        int count = I32(map + 0x08);
        int capacity = I32(map + 0x0C);
        int bitCount = I32(map + 0x28);

        if (count < 0 ||
            count > limit ||
            capacity < count ||
            capacity > limit ||
            bitCount < count ||
            bitCount > limit)
        {
            throw new InvalidOperationException(
                "Invalid sparse array.");
        }

        if (count == 0)
            return result;

        long bits = Ptr(map + 0x20);

        if (bits == 0)
        {
            if (bitCount > 128)
            {
                throw new InvalidOperationException(
                    "Invalid inline allocation flags.");
            }

            bits = map + 0x10;
        }

        if (!Pointer(data) || !Pointer(bits))
        {
            throw new InvalidOperationException(
                "Sparse array unavailable.");
        }

        byte[] flags = Read(
            bits,
            ((bitCount + 31) / 32) * 4);

        for (int i = 0; i < count; i++)
        {
            uint word = BitConverter.ToUInt32(
                flags,
                (i / 32) * 4);

            if ((word & (1u << (i % 32))) == 0)
                continue;

            result.Add(data + i * (long)stride);
        }

        if (Ptr(map) != data ||
            I32(map + 8) != count)
        {
            throw new InvalidOperationException(
                "Module collection changed during reading.");
        }

        return result;
    }

    private long GetBoatModuleSubsystem(Context c)
    {
        long map = c.World + 0x9F0;

        foreach (long entry in BoatSparseEntries(
            map, 0x18, 4096))
        {
            long candidate = Ptr(entry + 8);

            if (!Pointer(candidate))
                continue;

            if (!IsA(candidate, BoatModuleSubsystemClass))
                continue;

            if (Ptr(candidate + 0x20) != c.World)
                continue;

            if (BoatObjectEntry(candidate) == 0)
                continue;

            return candidate;
        }

        throw new InvalidOperationException(
            "Boat module subsystem not found.");
    }

    private bool IsActorOnBoat(long actor, long boat)
    {
        if (actor == boat)
            return true;

        if (!Pointer(actor) || !Pointer(boat))
            return false;

        long component = Ptr(actor + 0x1B8);
        long boatRoot = Ptr(boat + 0x1B8);

        if (!Pointer(boatRoot))
            return false;

        var visited = new HashSet<long>();

        for (int i = 0; i < 32; i++)
        {
            if (!Pointer(component) ||
                !visited.Add(component))
            {
                return false;
            }

            if (component == boatRoot)
                return true;

            long parent = Ptr(component + 0xD0);

            if (!Pointer(parent))
                return false;

            long owner = Ptr(parent + 0xA8);

            if (owner == boat)
                return true;

            component = parent;
        }

        return false;
    }

    private List<(long Actor, long Module)> GetBoatModules(
        Context c,
        long boat)
    {
        long subsystem = GetBoatModuleSubsystem(c);

        long map = subsystem + 0x110;

        var modules =
            new List<(long Actor, long Module)>();

        var visited = new HashSet<long>();

        foreach (long entry in BoatSparseEntries(
            map, 0x18))
        {
            try
            {
                long actor = Ptr(entry);
                long module = Ptr(entry + 8);

                if (!Pointer(actor) ||
                    !Pointer(module))
                {
                    continue;
                }

                if (!IsActorOnBoat(actor, boat))
                    continue;

                if (!IsA(module, ModuleClass))
                    continue;

                if (BoatObjectEntry(actor) == 0 ||
                    BoatObjectEntry(module) == 0)
                {
                    continue;
                }

                if (!visited.Add(module))
                    continue;

                modules.Add((actor, module));
            }
            catch
            {
                // Objects may be removed by the game
                // while the collection is being scanned.
            }
        }

        return modules;
    }

    // =========================================================
    // BOAT FUEL AND ELECTRICITY
    // =========================================================

    private double GetBoatResourceCapacity(
        long module,
        byte resource)
    {
        double capacity = 0;

        if (Byte(module + 0x110) == resource)
            capacity = Double(module + 0x148);

        foreach (long entry in BoatSparseEntries(
            module + 0x9A8, 0x20, 4096))
        {
            if (Byte(entry + 8) == resource)
                capacity += Double(entry + 0x10);
        }

        if (!double.IsFinite(capacity) ||
            capacity < 0 ||
            capacity > 1e9)
        {
            throw new InvalidOperationException(
                $"Invalid resource capacity: {capacity}.");
        }

        return capacity;
    }

    private (
        long Address,
        double Value,
        double Capacity)?
        GetBoatResource(long module, byte resource)
    {
        if (!IsA(module, ModuleClass))
            return null;

        long vtable = Ptr(module);

        // Confirm the module uses the original resource layout.
        if (Ptr(vtable + 0x548) != image + 0x554D6F0 ||
            Ptr(vtable + 0x588) != image + 0x554A640)
        {
            return null;
        }

        if (Byte(module + 0x975) == 0)
            return null;

        double capacity = GetBoatResourceCapacity(
            module,
            resource);

        if (capacity <= 0)
            return null;

        long map = module + 0x920;

        foreach (long entry in BoatSparseEntries(
            map, 0x18, 4096))
        {
            if (Byte(entry) != resource)
                continue;

            long address = entry + 8;

            double value = Double(address);

            if (!double.IsFinite(value) ||
                value < -1 ||
                value > 1e9)
            {
                throw new InvalidOperationException(
                    $"Invalid resource value: {value}.");
            }

            return (address, value, capacity);
        }

        return (0, 0, capacity);
    }

    private (int Fuel, int Electricity) RefillBoatResources(
        Context c,
        long boat,
        bool fuel,
        bool electricity,
        bool verbose = false,
        bool write = true)
    {
        var modules = GetBoatModules(c, boat);

        int fuelCount = 0;
        int electricityCount = 0;

        double fuelAmount = 0;
        double fuelCapacity = 0;

        double energyAmount = 0;
        double energyCapacity = 0;

        byte[] resources =
        {
            PetrolResource,
            DieselResource,
            ElectricityResource
        };

        foreach (var item in modules)
        {
            long actor = item.Actor;
            long module = item.Module;

            foreach (byte resource in resources)
            {
                bool isElectricity =
                    resource == ElectricityResource;

                if (isElectricity && !electricity)
                    continue;

                if (!isElectricity && !fuel)
                    continue;

                try
                {
                    var result = GetBoatResource(
                        module,
                        resource);

                    if (!result.HasValue)
                        continue;

                    var r = result.Value;

                    string resourceName = resource switch
                    {
                        ElectricityResource => "Electricity",
                        PetrolResource => "Petrol",
                        DieselResource => "Diesel",
                        _ => "Unknown"
                    };

                    if (r.Address == 0)
                    {
                        if (verbose)
                        {
                            Console.WriteLine(
                                $"{resourceName}: capacity " +
                                $"{r.Capacity:0.##}, but no " +
                                "allocated resource entry.");
                        }

                        continue;
                    }

                    double currentValue = r.Value;

                    if (write && r.Value < r.Capacity)
                    {
                        // Revalidate the boat, actor and module
                        // immediately before changing memory.
                        if (GetBoat(c) != boat ||
                            BoatObjectEntry(actor) == 0 ||
                            BoatObjectEntry(module) == 0 ||
                            !IsActorOnBoat(actor, boat))
                        {
                            continue;
                        }

                        var verified = GetBoatResource(
                            module,
                            resource);

                        if (!verified.HasValue ||
                            verified.Value.Address != r.Address)
                        {
                            continue;
                        }

                        r = verified.Value;

                        if (r.Value < r.Capacity)
                        {
                            WriteDouble(
                                r.Address,
                                r.Capacity);

                            freezeWrites++;
                        }

                        currentValue = Double(r.Address);
                    }

                    if (isElectricity)
                    {
                        electricityCount++;

                        energyAmount += currentValue;
                        energyCapacity += r.Capacity;
                    }
                    else
                    {
                        fuelCount++;

                        fuelAmount += currentValue;
                        fuelCapacity += r.Capacity;
                    }

                    if (verbose)
                    {
                        Console.WriteLine(
                            $"Boat {resourceName}: " +
                            $"{currentValue:0.##}/" +
                            $"{r.Capacity:0.##} " +
                            $"@ 0x{r.Address:X}");
                    }
                }
                catch (Exception ex)
                {
                    if (verbose)
                    {
                        Console.WriteLine(
                            $"Boat module 0x{module:X}: " +
                            ex.Message);
                    }
                }
            }
        }

        if (verbose)
        {
            Console.WriteLine(
                $"Boat modules found: {modules.Count}");

            Console.WriteLine(
                $"Total boat fuel: " +
                $"{fuelAmount:0.##}/{fuelCapacity:0.##}");

            Console.WriteLine(
                $"Total boat electricity: " +
                $"{energyAmount:0.##}/{energyCapacity:0.##}");

            Console.WriteLine(
                $"Fuel stores: {fuelCount} | " +
                $"Electricity stores: {electricityCount}");
        }

        return (fuelCount, electricityCount);
    }

    // =========================================================
    // MAXIMUM BOAT THROTTLE
    // =========================================================

    private void SetMaximumBoatThrottle(Context c)
    {
        long boat = GetBoat(c, helmOnly: true);

        if (boat == 0)
        {
            throw new InvalidOperationException(
                "Take control of the boat first.");
        }

        if (throttleBoat != 0 &&
            throttleBoat != boat)
        {
            throw new InvalidOperationException(
                "Controlled boat changed.");
        }

        long address = boat + BoatThrottleOffset;

        float currentThrottle = Float(address);

        if (!float.IsFinite(currentThrottle) ||
            currentThrottle < -1.01f ||
            currentThrottle > 1.01f)
        {
            throw new InvalidOperationException(
                $"Unexpected throttle value: {currentThrottle}.");
        }

        if (Ptr(c.Controller + 0x2E8) != boat)
        {
            throw new InvalidOperationException(
                "No longer controlling the boat.");
        }

        // The native throttle setter clamps inputs
        // between -1.0 and 1.0.
        throttleBoat = boat;

        if (currentThrottle != 1f)
        {
            WriteFloat(
                address,
                1f,
                verify: false);
        }
    }

    private void StopBoatThrottle(Context? c)
    {
        maximumBoatThrottle = false;

        try
        {
            if (c != null &&
                throttleBoat != 0 &&
                GetBoat(c) == throttleBoat)
            {
                WriteFloat(
                    throttleBoat + BoatThrottleOffset,
                    0f,
                    verify: false);

                Console.WriteLine(
                    "Boat throttle returned to neutral.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Boat throttle released: {ex.Message}");
        }
        finally
        {
            throttleBoat = 0;
        }
    }

    // =========================================================
    // BOAT FEATURE ACTIONS
    // =========================================================

    private void BoatAction(int id)
    {
        if (id == 18 && unlimitedBoatFuel)
        {
            unlimitedBoatFuel = false;

            Console.WriteLine(
                "Unlimited boat fuel: OFF");

            return;
        }

        if (id == 19 && unlimitedBoatElectricity)
        {
            unlimitedBoatElectricity = false;

            Console.WriteLine(
                "Unlimited boat electricity: OFF");

            return;
        }

        if (id == 20 && maximumBoatThrottle)
        {
            StopBoatThrottle(Live());

            Console.WriteLine(
                "Maximum boat throttle: OFF");

            return;
        }

        var c = Live()
            ?? throw new InvalidOperationException(
                "No verified player.");

        long boat = GetBoat(c);

        if (boat == 0)
        {
            throw new InvalidOperationException(
                "Take control of the boat first.");
        }

        switch (id)
        {
            case 17:
                {
                    var result = RefillBoatResources(
                        c,
                        boat,
                        fuel: true,
                        electricity: false,
                        verbose: true);

                    if (result.Fuel == 0)
                    {
                        throw new InvalidOperationException(
                            "No writable boat fuel stores found.");
                    }

                    Console.WriteLine("Boat fuel refilled.");

                    break;
                }

            case 18:
                {
                    var result = RefillBoatResources(
                        c,
                        boat,
                        fuel: true,
                        electricity: false,
                        verbose: true);

                    if (result.Fuel == 0)
                    {
                        throw new InvalidOperationException(
                            "No writable boat fuel stores found.");
                    }

                    unlimitedBoatFuel = true;
                    boatError = null;

                    Console.WriteLine(
                        "Unlimited boat fuel: ON");

                    break;
                }

            case 19:
                {
                    var result = RefillBoatResources(
                        c,
                        boat,
                        fuel: false,
                        electricity: true,
                        verbose: true);

                    if (result.Electricity == 0)
                    {
                        throw new InvalidOperationException(
                            "No writable boat electricity stores found.");
                    }

                    unlimitedBoatElectricity = true;
                    boatError = null;

                    Console.WriteLine(
                        "Unlimited boat electricity: ON");

                    break;
                }

            case 20:
                {
                    SetMaximumBoatThrottle(c);

                    maximumBoatThrottle = true;

                    Console.WriteLine(
                        "Maximum boat throttle: ON");

                    break;
                }
        }
    }

    private void TickBoat(Context? c)
    {
        if (!unlimitedBoatFuel &&
            !unlimitedBoatElectricity &&
            !maximumBoatThrottle)
        {
            return;
        }

        if (c == null)
        {
            if (maximumBoatThrottle)
                StopBoatThrottle(null);

            return;
        }

        // Refresh maximum throttle every 20 ms.
        if (maximumBoatThrottle &&
            boatThrottleTimer.ElapsedMilliseconds >= 20)
        {
            boatThrottleTimer.Restart();

            try
            {
                long boat = GetBoat(
                    c,
                    helmOnly: true);

                if (boat == 0 || boat != throttleBoat)
                {
                    StopBoatThrottle(c);

                    Console.WriteLine(
                        "Maximum throttle disabled: " +
                        "player left the helm.");
                }
                else
                {
                    SetMaximumBoatThrottle(c);
                }
            }
            catch (Exception ex)
            {
                StopBoatThrottle(c);

                Console.WriteLine(
                    $"Maximum throttle disabled: {ex.Message}");
            }
        }

        // Avoid scanning every module every 5 ms.
        if (boatResourceTimer.ElapsedMilliseconds < 750)
            return;

        boatResourceTimer.Restart();

        if (!unlimitedBoatFuel &&
            !unlimitedBoatElectricity)
        {
            return;
        }

        try
        {
            long boat = GetBoat(c);

            if (boat == 0)
            {
                throw new InvalidOperationException(
                    "Waiting for verified boat.");
            }

            var result = RefillBoatResources(
                c,
                boat,
                unlimitedBoatFuel,
                unlimitedBoatElectricity);

            if (unlimitedBoatFuel &&
                result.Fuel == 0)
            {
                throw new InvalidOperationException(
                    "Waiting for boat fuel stores.");
            }

            if (unlimitedBoatElectricity &&
                result.Electricity == 0)
            {
                throw new InvalidOperationException(
                    "Waiting for boat electricity stores.");
            }

            boatError = null;
        }
        catch (Exception ex)
        {
            if (boatError != ex.Message)
            {
                Console.WriteLine(
                    $"Boat resources paused: {ex.Message}");
            }

            boatError = ex.Message;
        }
    }

    private void PrintBoatStatus(Context c)
    {
        try
        {
            long boat = GetBoat(c);

            if (boat == 0)
            {
                Console.WriteLine(
                    "Boat not verified. " +
                    "Take control of the helm.");

                return;
            }

            Console.WriteLine(
                "========== BOAT DIAGNOSTICS ==========");

            Console.WriteLine(
                $"Boat: 0x{boat:X}");

            Console.WriteLine(
                $"Throttle: " +
                $"{Float(boat + BoatThrottleOffset):P0}");

            Console.WriteLine(
                $"Engine power: " +
                $"{Float(boat + BoatEnginePowerOffset):0.##}");

            RefillBoatResources(
                c,
                boat,
                fuel: true,
                electricity: true,
                verbose: true,
                write: false);

            Console.WriteLine(
                $"Unlimited fuel: {unlimitedBoatFuel}");

            Console.WriteLine(
                $"Unlimited electricity: " +
                $"{unlimitedBoatElectricity}");

            Console.WriteLine(
                $"Maximum throttle: {maximumBoatThrottle}");

            Console.WriteLine(
                "======================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Boat diagnostics: {ex.Message}");
        }
    }

    // =========================================================
    // WEATHER
    // =========================================================

    private void ToggleWeather(int id)
    {
        if (!WeatherPatches.TryGetValue(id, out var patches))
            return;

        var enabled = new List<CodePatch>();
        var pending = new List<CodePatch>();

        foreach (var patch in patches)
        {
            long address = image + patch.Rva;

            byte[] actual = Read(
                address,
                patch.Original.Length);

            if (actual.SequenceEqual(patch.Enabled))
            {
                enabled.Add(patch);
            }
            else if (actual.SequenceEqual(patch.Original))
            {
                pending.Add(patch);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Unexpected bytes for {patch.Name}. " +
                    $"Actual: {Convert.ToHexString(actual)}");
            }
        }

        appliedWeather[id] = enabled;

        if (enabled.Count == patches.Length)
        {
            RestoreWeather(id);

            Console.WriteLine(
                appliedWeather.ContainsKey(id)
                    ? $"Weather feature {id}: Restoration incomplete"
                    : $"Weather feature {id}: OFF");

            return;
        }

        try
        {
            foreach (var patch in pending)
            {
                enabled.Add(patch);

                WriteCode(
                    image + patch.Rva,
                    patch.Original,
                    patch.Enabled);
            }

            Console.WriteLine($"Weather feature {id}: ON");
        }
        catch
        {
            RestoreWeather(id);
            throw;
        }
    }

    private void RestoreWeather(int id)
    {
        if (!appliedWeather.TryGetValue(id, out var patches))
            return;

        if (process.HasExited)
        {
            appliedWeather.Remove(id);
            return;
        }

        foreach (var patch in patches.ToArray().Reverse())
        {
            try
            {
                long address = image + patch.Rva;

                byte[] actual = Read(
                    address,
                    patch.Original.Length);

                if (!actual.SequenceEqual(patch.Original))
                {
                    WriteCode(
                        address,
                        patch.Enabled,
                        patch.Original);
                }

                patches.Remove(patch);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Could not restore {patch.Name}: {ex.Message}");
            }
        }

        if (patches.Count == 0)
            appliedWeather.Remove(id);
    }

    private void RestoreAllWeather()
    {
        foreach (int id in appliedWeather.Keys.ToArray())
            RestoreWeather(id);
    }

    private long ValidWeather(Context c)
    {
        if (weatherWorld != c.World ||
            !IsA(weatherObject, WeatherClass) ||
            Ptr(weatherObject + 0x20) != c.World)
        {
            throw new InvalidOperationException(
                "Weather subsystem not verified.");
        }

        return weatherObject;
    }

    private void ResolveWeather(Context c)
    {
        try
        {
            ValidWeather(c);
            return;
        }
        catch
        {
            weatherObject = 0;
            weatherWorld = 0;
        }

        long cls = Ptr(image + WeatherClass);

        if (!Pointer(cls))
        {
            throw new InvalidOperationException(
                "Weather class not initialized.");
        }

        long map = c.World + 0x9F0;

        var matches = new HashSet<long>();

        foreach (long entry in BoatSparseEntries(
            map, 0x18, 4096))
        {
            long registeredClass = Ptr(entry);
            long candidate = Ptr(entry + 8);

            if (!Pointer(candidate))
                continue;

            long actualClass = Ptr(candidate + 0x10);
            long outer = Ptr(candidate + 0x20);

            if (!IsA(candidate, WeatherClass) ||
                actualClass != registeredClass ||
                outer != c.World)
            {
                continue;
            }

            matches.Add(candidate);
        }

        if (matches.Count != 1)
        {
            throw new InvalidOperationException(
                $"Expected one weather subsystem, " +
                $"found {matches.Count}.");
        }

        weatherObject = matches.Single();
        weatherWorld = c.World;

        Console.WriteLine(
            $"Weather subsystem verified: " +
            $"0x{weatherObject:X}");
    }

    private float DaylightTime(Context c)
    {
        long w = ValidWeather(c);

        float sunrise = Float(w + 0x22C);
        float sunset = Float(w + 0x230);

        if (!float.IsFinite(sunrise) ||
            !float.IsFinite(sunset) ||
            sunrise < 0 ||
            sunset <= sunrise ||
            sunset > 86400)
        {
            throw new InvalidOperationException(
                "Unexpected sunrise/sunset values.");
        }

        return sunrise + (sunset - sunrise) / 2f;
    }

    private (long Data, int Count) WaveArray(Context c)
    {
        long w = ValidWeather(c);

        long data = Ptr(w + 0x100);

        int count = I32(w + 0x108);
        int capacity = I32(w + 0x10C);

        if (count < 0 ||
            count > 128 ||
            capacity < count ||
            capacity > 1024 ||
            (count != 0 && !Pointer(data)))
        {
            throw new InvalidOperationException(
                "Invalid wave array.");
        }

        return (data, count);
    }

    // =========================================================
    // PLAYER MODIFIERS
    // =========================================================

    private long Address(Context c, Field f)
    {
        if (f.Owner == 7)
        {
            var waves = WaveArray(c);

            if (f.Offset < 0 ||
                f.Offset / WaveStride >= waves.Count)
            {
                throw new InvalidOperationException(
                    "Wave array changed.");
            }

            return waves.Data + f.Offset;
        }

        long p = f.Owner switch
        {
            0 => c.Movement,
            2 => c.Weight,
            3 => c.Settings,
            4 => c.Camera,
            5 => c.Pawn,
            6 => ValidWeather(c),
            _ => 0
        };

        if (!Pointer(p))
        {
            throw new InvalidOperationException(
                "Required component unavailable.");
        }

        return p + f.Offset;
    }

    private static bool Reasonable(Field f, float value)
    {
        return float.IsFinite(value) &&
               value >= (f.Owner == 5 ? -1_000_000 : 0) &&
               value <= (f.Owner is 2 or 5
                   ? 10_000_000
                   : 1_000_000) &&
               (f.Owner != 4 ||
                   (value >= 10 && value <= 180)) &&
               (f.Owner != 3 ||
                   (value > 0 && value <= 20));
    }

    private float BaseCarry(Context c)
    {
        long settings = Ptr(c.Pawn + 0x800);

        if (settings == 0)
            return 0;

        if (!Pointer(settings))
        {
            throw new InvalidOperationException(
                "Invalid carry capacity settings.");
        }

        float value = Float(settings + 0x3C);

        if (!float.IsFinite(value) ||
            value < 0 ||
            value > CarryCapacityKg)
        {
            throw new InvalidOperationException(
                $"Unexpected base capacity: {value}.");
        }

        return value;
    }

    private float EffectiveCarry(Context c)
    {
        if (!Pointer(c.Weight))
        {
            throw new InvalidOperationException(
                "Inventory weight unavailable.");
        }

        return Float(c.Weight + 0x4F0);
    }

    private void PrintCarryStatus(Context c)
    {
        try
        {
            float baseKg = BaseCarry(c);
            float bonusKg = Float(c.Pawn + 0x9E4);

            Console.WriteLine(
                $"Carry: {baseKg + bonusKg:N2} kg " +
                $"(base {baseKg:N2}, bonus {bonusKg:N2}); " +
                $"inventory effective {EffectiveCarry(c):N2} kg.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Carry diagnostic: {ex.Message}");
        }
    }

    private void Apply(
        int id,
        Saved s,
        Context? verified = null)
    {
        var c = verified ?? Live();

        if (c == null ||
            (s.Field.Owner is 6 or 7
                ? c.World != s.Context.World
                : c.Pawn != s.Context.Pawn) ||
            Address(c, s.Field) != s.Address ||
            !Reasonable(s.Field, Float(s.Address)))
        {
            throw new InvalidOperationException(
                "Field or player is no longer valid; " +
                "re-enable this option.");
        }

        float target = s.Field.Factor == 0
            ? s.Field.Absolute
            : s.Original * s.Field.Factor;

        if (multipliers.TryGetValue(
            id,
            out float multiplier))
        {
            target = id == 11
                ? multiplier
                : s.Original * multiplier;
        }

        if (s.Field.Owner is 2 or 5)
        {
            // Validate the real character, not the possessed boat.
            if (GetCharacterOrDriver(c.Controller) != c.Pawn ||
                Ptr(c.Pawn + 0x6B8) != c.Weight)
            {
                throw new InvalidOperationException(
                    "Carry component or owner changed.");
            }

            if (s.Field.Owner == 5)
                target = CarryCapacityKg - BaseCarry(c);
        }

        if (Float(s.Address) != target)
        {
            WriteFloat(
                s.Address,
                target,
                verify: false);
        }
    }

    private void Restore(IEnumerable<Saved> saved)
    {
        foreach (var s in saved)
        {
            try
            {
                bool weather = s.Field.Owner is 6 or 7;

                var c = Live();

                if (c == null)
                    continue;

                if ((weather
                        ? c.World == s.Context.World
                        : c.Pawn == s.Context.Pawn) &&
                    Address(c, s.Field) == s.Address &&
                    Reasonable(s.Field, Float(s.Address)))
                {
                    WriteFloat(
                        s.Address,
                        s.Original);
                }
            }
            catch
            {
                Console.WriteLine(
                    $"Could not restore 0x{s.Address:X} safely.");
            }
        }
    }

    private void RestoreAll()
    {
        if (maximumBoatThrottle)
            StopBoatThrottle(Live());

        unlimitedBoatFuel = false;
        unlimitedBoatElectricity = false;

        unlimitedHealth = false;
        unlimitedBattery = false;

        healthError = null;
        batteryError = null;
        boatError = null;

        lastBoat = 0;
        lastBoatWorld = 0;
        lastBoatPlayer = 0;
        lastBoatSerial = 0;

        RestoreFreeCraft();
        RestoreHumanSeedDecay();

        RestoreAllWeather();

        foreach (var s in active.Values)
            Restore(s);

        active.Clear();
    }

    // =========================================================
    // FREE CRAFTING
    // =========================================================

    private void ToggleFreeCraft()
    {
        var enabledPatches = new List<CodePatch>();
        var pendingPatches = new List<CodePatch>();

        // Determine the actual state of every instruction.
        foreach (var patch in FreeCraftPatches)
        {
            long address = image + patch.Rva;

            byte[] actual = Read(
                address,
                patch.Original.Length);

            if (actual.SequenceEqual(patch.Enabled))
            {
                enabledPatches.Add(patch);
            }
            else if (actual.SequenceEqual(patch.Original))
            {
                pendingPatches.Add(patch);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Unexpected bytes for {patch.Name} " +
                    $"at 0x{address:X}. " +
                    $"Actual: {BitConverter.ToString(actual)}. " +
                    $"Original: {BitConverter.ToString(patch.Original)}. " +
                    $"Enabled: {BitConverter.ToString(patch.Enabled)}.");
            }
        }

        // Synchronise internal state with actual game memory.
        appliedCraft.Clear();
        appliedCraft.AddRange(enabledPatches);

        // Everything is already enabled: toggle OFF.
        if (enabledPatches.Count == FreeCraftPatches.Length)
        {
            RestoreFreeCraft();

            Console.WriteLine(
                appliedCraft.Count == 0
                    ? "Free crafting: OFF"
                    : "Free crafting: restoration incomplete.");

            return;
        }

        // Enable only patches that have not yet been applied.
        try
        {
            foreach (var patch in pendingPatches)
            {
                appliedCraft.Add(patch);

                WriteCode(
                    image + patch.Rva,
                    patch.Original,
                    patch.Enabled);
            }

            Console.WriteLine(
                $"Free crafting: ON " +
                $"({appliedCraft.Count}/{FreeCraftPatches.Length} patches)");
        }
        catch
        {
            RestoreFreeCraft();
            throw;
        }
    }

    private void ToggleHumanSeedDecay()
    {
        var patch = HumanSeedDecayPatch;
        long address = image + patch.Rva;

        byte[] actual = Read(address, patch.Original.Length);

        if (actual.SequenceEqual(patch.Enabled))
        {
            humanSeedFrozen = true;
            RestoreHumanSeedDecay();
            return;
        }

        if (!actual.SequenceEqual(patch.Original))
        {
            throw new InvalidOperationException(
                $"Unexpected bytes for {patch.Name}. " +
                $"Actual: {BitConverter.ToString(actual)}");
        }

        humanSeedFrozen = true;

        try
        {
            WriteCode(
                address,
                patch.Original,
                patch.Enabled);

            Console.WriteLine("Unlimited human seed lifetime: ON");
        }
        catch
        {
            RestoreHumanSeedDecay();
            throw;
        }
    }

    private void RestoreHumanSeedDecay()
    {
        if (!humanSeedFrozen)
            return;

        try
        {
            if (!process.HasExited)
            {
                var patch = HumanSeedDecayPatch;
                long address = image + patch.Rva;

                byte[] actual = Read(
                    address,
                    patch.Original.Length);

                if (actual.SequenceEqual(patch.Enabled))
                {
                    WriteCode(
                        address,
                        patch.Enabled,
                        patch.Original);
                }
                else if (!actual.SequenceEqual(patch.Original))
                {
                    throw new InvalidOperationException(
                        "Unexpected human seed decay bytes.");
                }
            }

            humanSeedFrozen = false;

            Console.WriteLine("Unlimited human seed lifetime: OFF");
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Could not restore human seed decay: {ex.Message}");
        }
    }


    private void RestoreFreeCraft()
    {
        if (appliedCraft.Count == 0)
            return;

        if (process.HasExited)
        {
            appliedCraft.Clear();
            return;
        }

        foreach (var patch in appliedCraft.ToArray().Reverse())
        {
            try
            {
                long address = image + patch.Rva;

                byte[] currentBytes = Read(
                    address,
                    patch.Original.Length);

                if (!currentBytes.SequenceEqual(
                    patch.Original))
                {
                    WriteCode(
                        address,
                        patch.Enabled,
                        patch.Original);
                }

                appliedCraft.Remove(patch);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Could not restore {patch.Name}: " +
                    $"{ex.Message}. Restart the game.");
            }
        }
    }

    private void WriteCode(
        long address,
        byte[] expected,
        byte[] replacement)
    {
        if (expected.Length != replacement.Length ||
            !Read(address, expected.Length)
                .SequenceEqual(expected))
        {
            throw new InvalidOperationException(
                $"Unexpected instruction bytes at " +
                $"0x{address:X}.");
        }

        IntPtr p = new(address);

        UIntPtr size =
            (UIntPtr)(uint)replacement.Length;

        if (!VirtualProtectEx(
            handle,
            p,
            size,
            0x40,
            out uint oldProtect))
        {
            throw new InvalidOperationException(
                $"Cannot change code protection at " +
                $"0x{address:X}.");
        }

        try
        {
            if (!WriteProcessMemory(
                handle,
                p,
                replacement,
                size,
                out var written) ||
                written != size ||
                !Read(address, replacement.Length)
                    .SequenceEqual(replacement))
            {
                throw new InvalidOperationException(
                    $"Code patch failed at 0x{address:X}.");
            }

            if (!FlushInstructionCache(handle, p, size))
            {
                throw new InvalidOperationException(
                    "Cannot flush instruction cache.");
            }
        }
        finally
        {
            if (!VirtualProtectEx(
                handle,
                p,
                size,
                oldProtect,
                out _))
            {
                Console.WriteLine(
                    "WARNING: Could not restore memory " +
                    "protection. Restart the game.");
            }
        }
    }

    // =========================================================
    // HEALTH
    // =========================================================

    private (long Address, float Value, float Max)
        Attribute(Context c, byte id)
    {
        long map = c.Survival + 0x240;
        long data = Ptr(map);

        int count = I32(map + 8);
        int capacity = I32(map + 12);
        int free = I32(map + 0x34);

        int hashSize = I32(map + 0x48);

        long hash = Ptr(map + 0x40);

        if (hash == 0)
            hash = map + 0x38;

        if (!Pointer(data) ||
            count < 1 ||
            count > 1024 ||
            capacity < count ||
            capacity > 4096 ||
            free < 0 ||
            free >= count ||
            hashSize < 1 ||
            hashSize > 4096 ||
            (hashSize & (hashSize - 1)) != 0)
        {
            throw new InvalidOperationException(
                "Survival map not initialized.");
        }

        if (hash == map + 0x38 && hashSize > 2)
        {
            throw new InvalidOperationException(
                "Invalid inline survival hash.");
        }

        int index = I32(
            hash + (id & (hashSize - 1)) * 4L);

        var visited = new HashSet<int>();

        while (index != -1)
        {
            if (index < 0 ||
                index >= count ||
                !visited.Add(index))
            {
                throw new InvalidOperationException(
                    "Invalid survival hash chain.");
            }

            long entry = data + index * 0x98L;

            if (Byte(entry) == id)
            {
                float value = Float(entry + 8);
                float min = Float(entry + 0x10);
                float max = Float(entry + 0x14);

                if (Byte(entry + 0x0C) != id ||
                    !float.IsFinite(value) ||
                    !float.IsFinite(min) ||
                    !float.IsFinite(max) ||
                    max <= min ||
                    max > 1000000 ||
                    value < min - 1 ||
                    value > max + 1)
                {
                    throw new InvalidOperationException(
                        $"Attribute {id} failed validation.");
                }

                return (entry + 8, value, max);
            }

            index = I32(entry + 0x90);
        }

        throw new InvalidOperationException(
            $"Attribute {id} unavailable.");
    }

    private void Refill(Context c, byte id)
    {
        long data = Ptr(c.Survival + 0x240);

        var a = Attribute(c, id);

        if (GetCharacterOrDriver(c.Controller) != c.Pawn ||
            Ptr(c.Pawn + 0x698) != c.Survival ||
            Ptr(c.Survival + 0x240) != data ||
            Byte(a.Address - 8) != id)
        {
            throw new InvalidOperationException(
                "Health allocation or player changed; retrying.");
        }

        if (a.Value < a.Max)
        {
            WriteFloat(
                a.Address,
                a.Max,
                verify: false);

            freezeWrites++;
        }

        if (id == 8)
            healthError = null;
    }

    // =========================================================
    // PLAYER BATTERY
    // =========================================================

    private (long Address, double Value, double Max)
        Battery(Context c)
    {
        long module = Ptr(c.Pawn + 0x6C8);

        if (!IsA(module, ModuleClass))
        {
            throw new InvalidOperationException(
                "Player energy module unavailable.");
        }

        long vtable = Ptr(module);

        if (Ptr(vtable + 0x548) != image + 0x554D6F0 ||
            Ptr(vtable + 0x588) != image + 0x554A640)
        {
            throw new InvalidOperationException(
                "Unsupported player energy module layout.");
        }

        long map = module + 0x920;
        long data = Ptr(map);

        long hash = Ptr(map + 0x40);

        int count = I32(map + 8);
        int capacity = I32(map + 12);
        int hashSize = I32(map + 0x48);

        if (hash == 0)
        {
            if (hashSize > 2)
            {
                throw new InvalidOperationException(
                    "Invalid energy hash.");
            }

            hash = map + 0x38;
        }

        if (!Pointer(data) ||
            !Pointer(hash) ||
            count < 1 ||
            count > 1024 ||
            capacity < count ||
            capacity > 4096 ||
            hashSize < 1 ||
            hashSize > 4096 ||
            (hashSize & (hashSize - 1)) != 0)
        {
            throw new InvalidOperationException(
                "Energy resource map unavailable.");
        }

        int index = I32(hash);

        var visited = new HashSet<int>();

        long address = 0;

        while (index != -1)
        {
            if (index < 0 ||
                index >= count ||
                !visited.Add(index))
            {
                throw new InvalidOperationException(
                    "Invalid energy resource hash chain.");
            }

            long entry = data + index * 0x18L;

            if (Byte(entry) == ElectricityResource)
            {
                address = entry + 8;
                break;
            }

            index = I32(entry + 0x10);
        }

        if (address == 0)
        {
            throw new InvalidOperationException(
                "Electricity not present in player module.");
        }

        double max = GetBoatResourceCapacity(
            module,
            ElectricityResource);

        double value = Double(address);

        if (!double.IsFinite(max) ||
            !double.IsFinite(value) ||
            max <= 0 ||
            max > 1e9 ||
            value < -1 ||
            value > 1e9)
        {
            throw new InvalidOperationException(
                $"Unexpected electricity: {value}/{max}.");
        }

        return (address, value, max);
    }

    private void RefillBattery(Context c)
    {
        long module = Ptr(c.Pawn + 0x6C8);

        if (!Pointer(module))
        {
            throw new InvalidOperationException(
                "Player energy module unavailable.");
        }

        long data = Ptr(module + 0x920);

        var b = Battery(c);

        if (GetCharacterOrDriver(c.Controller) != c.Pawn ||
            Ptr(c.Pawn + 0x6C8) != module ||
            Ptr(module + 0x920) != data ||
            Byte(b.Address - 8) != ElectricityResource)
        {
            throw new InvalidOperationException(
                "Electricity allocation or player changed.");
        }

        if (b.Value < b.Max)
        {
            WriteDouble(
                b.Address,
                b.Max);

            freezeWrites++;
        }

        batteryError = null;
    }

    // =========================================================
    // DIAGNOSTICS
    // =========================================================

    private void PrintStatus()
    {
        Console.WriteLine(
            "========== TRAINER DIAGNOSTICS ==========");

        Console.WriteLine(
          $"Free & instant crafting: " +
          (appliedCraft.Count == 0
              ? "OFF"
              : appliedCraft.Count == FreeCraftPatches.Length
                  ? "ON"
                  : "PARTIAL"));

            Console.WriteLine(
                $"Unlimited human seed lifetime: " +
                $"{(humanSeedFrozen ? "ON" : "OFF")}");

        var c = Live();

        if (c == null)
        {
            Console.WriteLine(
                $"No verified player: {validationError}");

            return;
        }

        Console.WriteLine(
            $"World 0x{c.World:X}, " +
            $"Pawn 0x{c.Pawn:X}, " +
            $"Movement 0x{c.Movement:X}, " +
            $"Survival 0x{c.Survival:X}");

        try
        {
            Console.WriteLine(
                $"Controlled pawn: " +
                $"0x{Ptr(c.Controller + 0x2E8):X}");
        }
        catch
        {
        }

        try
        {
            var h = Attribute(c, 8);

            Console.WriteLine(
                $"Health {h.Value:0.##}/{h.Max:0.##} " +
                $"@ 0x{h.Address:X}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Health: {ex.Message}");
        }

        try
        {
            var b = Battery(c);

            Console.WriteLine(
                $"Battery {b.Value:0.##}/{b.Max:0.##} " +
                $"@ 0x{b.Address:X}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Battery: {ex.Message}");
        }

        PrintCarryStatus(c);

        foreach (var (id, toggle) in Toggles)
        {
            foreach (var field in toggle.Fields)
            {
                try
                {
                    long address = Address(c, field);

                    Console.WriteLine(
                        $"{toggle.Name}: " +
                        $"{Float(address):0.###} " +
                        $"@ 0x{address:X} " +
                        $"[{(active.ContainsKey(id)
                            ? "ON"
                            : "OFF")}]");
                }
                catch
                {
                    Console.WriteLine(
                        $"{toggle.Name}: unavailable");
                }
            }
        }

        PrintBoatStatus(c);
    }

    // =========================================================
    // MEMORY HELPERS
    // =========================================================

    private bool IsA(long p, long classRva)
    {
        if (!Pointer(p))
            return false;

        long expected = Ptr(image + classRva);

        if (!Pointer(expected))
            return false;

        long actual = Ptr(p + 0x10);

        for (int i = 0;
             i < 48 && Pointer(actual);
             i++)
        {
            if (actual == expected)
                return true;

            actual = Ptr(actual + 0x40);
        }

        return false;
    }

    private static bool Pointer(long address)
    {
        return address >= 0x10000 &&
               address < 0x0000800000000000 &&
               (address & 7) == 0;
    }

    private long OptionalPointer(long address)
    {
        try
        {
            return Ptr(address);
        }
        catch
        {
            return 0;
        }
    }

    private long OptionalComponent(
        long address,
        long type)
    {
        try
        {
            long p = Ptr(address);

            return IsA(p, type) ? p : 0;
        }
        catch
        {
            return 0;
        }
    }

    private byte[]? TryRead(long address, int length)
    {
        if (address < 0x10000 || length <= 0)
            return null;

        var buffer = new byte[length];

        bool success = ReadProcessMemory(
            handle,
            new IntPtr(address),
            buffer,
            (UIntPtr)(uint)length,
            out var read);

        return success &&
               read.ToUInt64() == (ulong)length
            ? buffer
            : null;
    }

    private byte[] Read(long address, int length)
    {
        return TryRead(address, length)
            ?? throw new InvalidOperationException(
                $"Cannot read 0x{address:X}.");
    }

    private long Ptr(long address)
        => BitConverter.ToInt64(Read(address, 8));

    private int I32(long address)
        => BitConverter.ToInt32(Read(address, 4));

    private uint U32(long address)
        => BitConverter.ToUInt32(Read(address, 4));

    private byte Byte(long address)
        => Read(address, 1)[0];

    private float Float(long address)
        => BitConverter.ToSingle(Read(address, 4));

    private double Double(long address)
        => BitConverter.ToDouble(Read(address, 8));

    private void WriteDouble(long address, double value)
    {
        if (!double.IsFinite(value) ||
            (address & 7) != 0)
        {
            throw new InvalidOperationException(
                "Invalid double write.");
        }

        byte[] buffer = BitConverter.GetBytes(value);

        if (!WriteProcessMemory(
            handle,
            new IntPtr(address),
            buffer,
            (UIntPtr)8,
            out var written) ||
            written.ToUInt64() != 8)
        {
            throw new InvalidOperationException(
                $"Double write failed at 0x{address:X}. " +
                $"Win32 {Marshal.GetLastWin32Error()}.");
        }
    }

    private void WriteFloat(
        long address,
        float value,
        bool verify = true)
    {
        if (!float.IsFinite(value) ||
            (address & 3) != 0)
        {
            throw new InvalidOperationException(
                "Invalid float write.");
        }

        byte[] buffer = BitConverter.GetBytes(value);

        if (!WriteProcessMemory(
            handle,
            new IntPtr(address),
            buffer,
            (UIntPtr)4,
            out var written) ||
            written.ToUInt64() != 4)
        {
            throw new InvalidOperationException(
                $"Float write failed at 0x{address:X}. " +
                $"Win32 {Marshal.GetLastWin32Error()}.");
        }

        if (verify && Float(address) != value)
        {
            throw new InvalidOperationException(
                $"Game changed value at 0x{address:X}.");
        }
    }

    public void Dispose()
    {
        if (handle != IntPtr.Zero)
            CloseHandle(handle);

        process.Dispose();
    }

    // =========================================================
    // WINDOWS API
    // =========================================================

    [StructLayout(LayoutKind.Sequential)]
    private struct Mbi
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public uint AllocationProtect;
        public uint Padding;
        public UIntPtr RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
        public uint Padding2;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(
        uint access,
        bool inherit,
        int id);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(
        IntPtr handle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool ReadProcessMemory(
        IntPtr h,
        IntPtr address,
        [Out] byte[] buffer,
        UIntPtr size,
        out UIntPtr read);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WriteProcessMemory(
        IntPtr h,
        IntPtr address,
        byte[] buffer,
        UIntPtr size,
        out UIntPtr written);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern UIntPtr VirtualQueryEx(
        IntPtr h,
        IntPtr address,
        out Mbi info,
        UIntPtr size);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool VirtualProtectEx(
        IntPtr h,
        IntPtr address,
        UIntPtr size,
        uint newProtect,
        out uint oldProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FlushInstructionCache(
        IntPtr h,
        IntPtr address,
        UIntPtr size);

    [DllImport("winmm.dll")]
    private static extern uint timeBeginPeriod(
        uint period);

    [DllImport("winmm.dll")]
    private static extern uint timeEndPeriod(
        uint period);
}
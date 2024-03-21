using System;
using System.Collections.Generic;
using System.Linq;

public interface IHazardNotifier
{
    void NotifyHazard();
}

public abstract class Container
{
    public double Mass { get; protected set; }
    public int Height { get; }
    public double TareWeight { get; }
    public int Depth { get; }
    public string SerialNumber { get; }
    public double MaxPayload { get; }

    protected Container(double tareWeight, int height, int depth, string type, double maxPayload)
    {
        TareWeight = tareWeight;
        Height = height;
        Depth = depth;
        MaxPayload = maxPayload;
        SerialNumber = GenerateSerialNumber(type);
    }

    private static string GenerateSerialNumber(string type)
    {
        return $"KON-{type}-{new Random().Next(1000, 9999)}";
    }

    public virtual void LoadCargo(double mass)
    {
        if (mass > MaxPayload)
        {
            throw new OverfillException("Cargo exceeds max payload.");
        }
        Mass = mass;
    }

    public abstract void EmptyCargo();
}

public class LiquidContainer : Container, IHazardNotifier
{
    public bool IsHazardous { get; set; }

    public LiquidContainer(double tareWeight, int height, int depth, double maxPayload, bool isHazardous)
        : base(tareWeight, height, depth, "L", maxPayload)
    {
        IsHazardous = isHazardous;
    }

    public override void LoadCargo(double mass)
    {
        double maxLoad = IsHazardous ? MaxPayload * 0.5 : MaxPayload * 0.9;
        if (mass > maxLoad)
        {
            NotifyHazard();
            throw new OverfillException("Exceeding safe cargo limit.");
        }
        base.LoadCargo(mass);
    }

    public override void EmptyCargo()
    {
        Mass = 0;
    }

    public void NotifyHazard()
    {
        Console.WriteLine($"Hazardous condition detected in container {SerialNumber}.");
    }
}

public class GasContainer : Container, IHazardNotifier
{
    public double Pressure { get; set; }

    public GasContainer(double tareWeight, int height, int depth, double maxPayload, double pressure)
        : base(tareWeight, height, depth, "G", maxPayload)
    {
        Pressure = pressure;
    }

    public override void LoadCargo(double mass)
    {
        if (mass > MaxPayload)
        {
            NotifyHazard();
            throw new OverfillException("Cargo exceeds max payload.");
        }
        base.LoadCargo(mass);
    }

    public override void EmptyCargo()
    {
        Mass *= 0.05;
    }

    public void NotifyHazard()
    {
        Console.WriteLine($"Hazardous condition detected in container {SerialNumber}.");
    }
}

public class RefrigeratedContainer : Container
{
    public string ProductType { get; }
    public double Temperature { get; }

    public RefrigeratedContainer(double tareWeight, int height, int depth, string productType, double temperature, double maxPayload)
        : base(tareWeight, height, depth, "C", maxPayload)
    {
        ProductType = productType;
        Temperature = temperature;
    }

    public override void EmptyCargo()
    {
        Mass = 0;
    }
}

public class ContainerShip
{
    public List<Container> Containers { get; } = new List<Container>();
    public double MaxSpeed { get; }
    public int MaxContainerCount { get; }
    public double MaxWeight { get; }

    public ContainerShip(double maxSpeed, int maxContainerCount, double maxWeight)
    {
        MaxSpeed = maxSpeed;
        MaxContainerCount = maxContainerCount;
        MaxWeight = maxWeight;
    }

    public void LoadContainer(Container container)
    {
        if (Containers.Count >= MaxContainerCount || Containers.Sum(c => c.TareWeight + c.Mass) + container.TareWeight + container.Mass > MaxWeight)
        {
            throw new Exception("Cannot load more containers onto the ship.");
        }
        Containers.Add(container);
    }

    public void UnloadContainer(Container container)
    {
        if (Containers.Contains(container))
        {
            container.EmptyCargo();
            Containers.Remove(container);
        }
        else
        {
            throw new Exception("Container not found on the ship.");
        }
    }

    public void ReplaceContainer(Container existingContainer, Container newContainer)
    {
        if (Containers.Contains(existingContainer))
        {
            Containers.Remove(existingContainer);
            LoadContainer(newContainer);
        }
        else
        {
            throw new Exception("Container to replace not found on the ship.");
        }
    }

    public void TransferContainer(Container container, ContainerShip targetShip)
    {
        if (Containers.Contains(container))
        {
            targetShip.LoadContainer(container);
            Containers.Remove(container);
        }
        else
        {
            throw new Exception("Container not found for transfer.");
        }
    }

    public void PrintDetails()
    {
        Console.WriteLine($"Ship speed: {MaxSpeed} knots, Max containers: {MaxContainerCount}, Max weight: {MaxWeight} tons");
        foreach (var container in Containers)
        {
            Console.WriteLine($"Container serial: {container.SerialNumber}, Cargo mass: {container.Mass} kg, Type: {container.GetType().Name}");
        }
    }
}

public class OverfillException : Exception
{
    public OverfillException(string message) : base(message) { }
}

public class Program
{
    public static void Main(string[] args)
    {
        ContainerShip ship1 = new ContainerShip(30, 100, 20000);
        ContainerShip ship2 = new ContainerShip(25, 50, 15000);

        LiquidContainer milkContainer = new LiquidContainer(500, 300, 200, 1000, false);
        milkContainer.LoadCargo(800);

        GasContainer heliumContainer = new GasContainer(750, 250, 150, 1500, 100);
        heliumContainer.LoadCargo(1400);

        try
        {
            ship1.LoadContainer(milkContainer);
            ship1.LoadContainer(heliumContainer);
        }
        catch (OverfillException ex)
        {
            Console.WriteLine(ex.Message);
        }
        LiquidContainer oilContainer = new LiquidContainer(1000, 400, 300, 2000, true);
        oilContainer.LoadCargo(900);

        try
        {
            ship1.ReplaceContainer(milkContainer, oilContainer);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during replacement: {ex.Message}");
        }

        ship1.UnloadContainer(heliumContainer);

        try
        {
            ship1.TransferContainer(oilContainer, ship2);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during transfer: {ex.Message}");
        }
        Console.WriteLine("Ship 1 Details:");
        ship1.PrintDetails();
        Console.WriteLine("\nShip 2 Details:");
        ship2.PrintDetails();
    }
    
}

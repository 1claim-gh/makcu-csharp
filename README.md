# makcu-csharp

## DISCLAIMER
- **Requires System.IO.Ports NuGet Package**
- Install via .NET CLI using the command
- ```
  dotnet add package System.IO.Ports
  ```
- Can also be installed using the NuGet Package Manager in visual studio
## Basic C# Usage:
```csharp
using Mouse;

device.connect("COM1");
device.move(100, 100);
device.click(MouseButton.Left, 1);
```

## Performance (Results may vary)
- Mouse Movement (100 rapid moves tested): Total elapsed time: 46ms, (0.46 ms avg)
- Mouse Clicks (50 rapid clicks): Total elapsed time: 155ms, (1.55 ms avg)
- NOTE: Mouse clicks had a 1ms delay added between each command sent to ensure the command is sent
**On Average performs 10x Faster than most recent Python release (version 2.1.2 as of publish date)**

# makcu-csharp

## DISCLAIMER
- This was made with the intention for 2 PC Setup only
- This is for educational purposes only and i am not responsible for any bans, penalties or other consequences that you may encounter

## Prerequisites
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
- NOTE: Mouse clicks had a 1ms delay added between each command sent to ensure the command is sent.
 
**On Average performs 10x Faster than most recent Python release (version 2.1.2 as of publish date)**

## Acknowledgements

- [Makcu Discord Server](https://discord.gg/frvh3P4Qeg) community
- [Makcu C++ Library](https://github.com/K4HVH/makcu-cpp) by [K4HVH](https://github.com/K4HVH) (I had references previous versions of his C++ library when writing mine)
- [Makcu Python Library](https://github.com/SleepyTotem/makcu-py-lib) by [SleepyTotem](https://github.com/SleepyTotem)

# nved

**nved** is a tool to add, edit or remove User environment variables in the terminal.

## Usage

This project is only tested in Windows 10 but should also work in latest versions of Windows.
```
USAGE: ./nved
```

## Building

**NOTE**: This project requires the [.NET SDK](https://dotnet.microsoft.com/en-us/download).

1. Clone the repository:
```
git clone https://github.com/vishavish/nved.git
```

2.
> To run:

```
cd src
dotnet run
```
> To build:
```
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:PublishTrimmed=true --output build/windows
```

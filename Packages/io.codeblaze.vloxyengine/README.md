<p align="center">
  <img src="https://raw.githubusercontent.com/BLaZeKiLL/VloxyEngine/main/.github/assets/vloxy_logo.svg" width=256>
  <h1 align="center">Vloxy Engine</h1>
</p>

<p align="center">
  <a href="https://github.com/BLaZeKiLL/VloxyEngine/releases">
    <img alt="GitHub release (latest by date)" src="https://img.shields.io/github/v/release/BLaZeKiLL/VloxyEngine">
  </a>
  <a href="https://openupm.com/packages/io.codeblaze.vloxyengine/">
    <img alt="OpenUPM" src="https://img.shields.io/npm/v/io.codeblaze.vloxyengine?label=openupm&amp;registry_uri=https://package.openupm.com" />
  </a>
  <a href="https://github.com/BLaZeKiLL/VloxyEngine/actions">
    <img alt="Build Status" src="https://img.shields.io/github/workflow/status/BLaZeKiLL/VloxyEngine/Build">
  </a>
  <a href="https://blazekill.github.io/vloxy-docs/">
    <img alt="GitHub Workflow Status" src="https://img.shields.io/github/workflow/status/BLaZeKiLL/vloxy-docs/Deploy%20to%20GitHub%20Pages?label=docs">
  </a>
  <a href="https://github.com/BLaZeKiLL/VloxyEngine/blob/main/LICENSE.md">
    <img alt="GitHub" src="https://img.shields.io/github/license/BLaZeKiLL/VloxyEngine">
  </a>
  <a href="https://www.youtube.com/c/CodeBlazeX">
    <img alt="YouTube Channel Subscribers" src="https://img.shields.io/youtube/channel/subscribers/UC_qfPIYfXOvg0SDAc8Z68WA?label=CodeBlaze&style=social">
  </a>
</p>

Performance oriented voxel engine for unity. Latest release for the engine and sandbox application can be found [here](https://github.com/BLaZeKiLL/VloxyEngine/releases)

<img src="https://raw.githubusercontent.com/BLaZeKiLL/VloxyEngine/main/.github/assets/vloxy_engine.png">

## Goals

| Description                     | Done |
|---------------------------------|:----:|
| Jobs & Burst                    |  ✔   |
| Extensible Api                  |  ✔   |
| Serialization                   |      |
| Streaming & Infinite generation |  ✔   |
| Noise Generation System         |      |
| Physics & Fluids                |      |
| Networking                      |      |

## Demos
Along with package releases a demo application showcasing the capabilities is also released for the following platforms
- Windows
- Android

Head over to the [relase page](https://github.com/BLaZeKiLL/VloxyEngine/releases) to check them out

## Quick Start

- Get started by installing **Vloxy Engine** using one of the following methods
  - Unity Package latest can be found **[here](https://github.com/BLaZeKiLL/VloxyEngine/releases)**
  - OpenUPM, more info can be found **[here](https://openupm.com/packages/io.codeblaze.vloxyengine/)**
```bash title="OpenUPM Install Command"
openupm add io.codeblaze.vloxyengine
```

> While UPM is supported via OpenUPM, it is still recommended to add **Vloxy Engine** directly too the project as a package.
> With source access you would get the maximum control and freedom to tune the engine to your use case.


- Make sure the following dependencies are installed, they should be installed **automatically** regardless of the way you install **Vloxy Engine**
  - [Unity Maths](https://docs.unity3d.com/Packages/com.unity.mathematics@1.2/manual/index.html)
  - [Unity Burst](https://docs.unity3d.com/Packages/com.unity.burst@1.7/manual/index.html)
  - [Unity Collections](https://docs.unity3d.com/Packages/com.unity.collections@1.2/manual/index.html)

- After the package is imported you can open up one of the sample scene or import one of the world pre-fabe into your current scene.
in case you go the pre-fabe route make sure to set the focus parameter on the world object around which the world would be generated.

## Documentation & Dev-logs
Documentation can be found [here](https://blazekill.github.io/vloxy-docs/) and it's source code is hosted [here](https://github.com/BLaZeKiLL/vloxy-docs)

I'll try to create dev-logs for the major features in development as well as some tutorials which you can fin on my [youtube](https://www.youtube.com/c/CodeBlazeX) or on the vloxy engine [blog](https://blazekill.github.io/vloxy-docs/blog)


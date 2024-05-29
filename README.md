## Overview

TransCodeMD was a fun little project I picked up to keep my VB.NET skills fresh and, let's be honest, to distract myself from work for a while `¯\_(ツ)_/¯` . Despite being a work in progress, TransCodeMD has already implemented its core functionalities.

## Key Features

- **Source Code Transcoding:** TransCodeMD specializes in 'transcoding' - a process where it creates Markdown files from various source code files (such as .py, .cs, .vb, .ps1). These Markdown files act as simple wrappers or aliases for the original source code. You can easily include these alias Markdown files into other documentation, especially in applications like Obsidian that support transclusion.

- **File Monitoring and Synchronization:** TransCodeMD has a monitoring system that keeps track of changes in source code files. Whenever it detects modifications in the source code, it automatically updates the corresponding Markdown files to reflect these changes. This synchronization is one-way, ensuring that the Markdown files always mirror the latest version of the source code.

### Installation

1. Download the source code and compile the project to your preferred installation directory.
2. Add the file path to your system's PATH environment variable.
3. Use the `transcode` or `tc` aliases in your CLI to access TransCodeMD.

### Usage

Run `TransCodeMD.exe` with various options to leverage its features:

- `-a, --addsourcefiles`: Add all source files in the current directory to `.transclude`.
- `-d, --addrootpath`: Add the current directory as a root path in `.tconfig`.
- `-h, --help`: Display help information.
- `-l, --listrootpaths`: List all root paths in `.tconfig`.
- `-m, --runasmonitor`: Activate monitor mode.
- `-s, --sourcefile`: Add a single source file to `.transclude`.
- `-t, --listsourcefiles`: List all source files in `.transclude`.
- `-y, --syncfiles`: Sync all files in `.transclude`.

>To enable the use of `transcode` and `tc` aliases in the Windows command line interface (CLI), add the project's install directory to your root path in Windows.

## Project Status

TransCodeMD is actively being developed...kinda 🛠️

---

*Note: TransCodeMD is a project in flux. Features and documentation may evolve as development continues.*

## Contact Information

For any questions or feedback, feel free to reach out:

**LinkedIn:** [Braxton Bell](https://www.linkedin.com/in/braxton-bell/)

---


# README

---

## Overview

TransCodeMD is a simple VB.NET console application simply the inclussion for `source code` scripts in  Markdown documentation. Although TransCodeMD is in ongoing development, its main features are implemented.

## Key Features

- **Source Code Transcoding:** TransCodeMD specializes in 'transcoding' - a process where it creates simplified Markdown files from various source code files (such as .py, .cs, .vb, .ps1). These Markdown files serve as straightforward wrappers or aliases for the original source code. The alias Markdown representations can be easily transcluded into other documentation, compatible with applications that support transclusion like Obsidian.

- **File Monitoring and Synchronization:** TransCodeMD employs a monitoring system that tracks changes in source code files. Upon detecting modifications in the source code, it automatically updates the corresponding Markdown files to reflect these changes. This synchronization is one-directional, ensuring that the Markdown files are always an up-to-date representation of the source code.

### Installation

1. Download the source and compile the project to your desired install directory.
2. Add the file path to your system's PATH environment variable.
3. Use the `transcode` or `tc` alias in your CLI to access TransCodeMD.

### Usage

Run `TransCodeMD.exe` with various options to utilize its features:

- `-a, --addsourcefiles`: Add all source files in the current directory to `.transclude`.
- `-d, --addrootpath`: Add the current directory as a root path in `.tconfig`.
- `-h, --help`: Show help information.
- `-l, --listrootpaths`: List all root paths in `.tconfig`.
- `-m, --runasmonitor`: Run in monitor mode.
- `-s, --sourcefile`: Add a single source file to `.transclude`.
- `-t, --listsourcefiles`: List all source files in `.transclude`.
- `-y, --syncfiles`: Sync all files in `.transclude`.

>Add the project install directory to your root path in Windows to enable use of the aliases `transcode` and `tc` in the Windows command line interface (CLI).

## Project Status

TransCodeMD is currently in active development.

---

*Note: TransCodeMD is an evolving project. The features and documentation may change as development progresses.*

**[中文繁體](README_zh_TW.md) | [中文简体](README_zh_CN.md) | [English(US)](README_en_US.md)**
# FlickUAC
By adding the RunAsInvoker compatibility level to the registry, FlickUAC forces specific files to execute with the caller's privileges, effectively bypassing User Account Control (UAC) elevation requests.

## Core Features

### 1. Modern List Management
* **Automatic Resource Parsing**: The program automatically parses and extracts icons for corresponding executable paths, providing intuitive visual identification for quick item recognition.
* **Responsive Selection Logic**: The refactored selection mechanism ensures smooth operational feedback even when handling large-scale data.
* **High-Performance Architecture**: Developed based on .NET 10.0, ensuring high execution efficiency when processing system-level registry operations.

### 2. Advanced Batch Operations
* **Multi-Item Selection**: Supports standard Extended Selection mode, allowing users to select multiple targets via mouse drag, Shift, or Ctrl keys.
* **Synchronous Processing**: Supports simultaneous registry key deletion or file location retrieval for multiple selected items.

---

## Comparison Gallery

<table border="0">
  <tr>
    <td align="center"><b>Without FlickUAC</b></td>
    <td align="center"><b>With FlickUAC</b></td>
  </tr>
  <tr>
    <td>
        <p align="center">
        <img src="https://github.com/user-attachments/assets/25c42eab-ec19-4599-83a0-df780a7c9d58" width="30%" title="Before Launch" />
        <img src="https://github.com/user-attachments/assets/5e316e8f-d6ae-456e-979a-48a35a327794" width="30%" title="UAC Prompt" />
        <img src="https://github.com/user-attachments/assets/f91a81d2-4828-407a-bf8c-7bf7c34e7fd5" width="30%" title="After Launch" />
</p></td>
    <td><p align="center">
        <img src="https://github.com/user-attachments/assets/25c42eab-ec19-4599-83a0-df780a7c9d58" width="42%" title="Before Launch" />
        <img src="https://github.com/user-attachments/assets/f91a81d2-4828-407a-bf8c-7bf7c34e7fd5" width="42%" title="After Launch" />
</p></td>
  </tr>
  <tr>
    <td>System triggers UAC block</td>
    <td>Program launches directly without interference</td>
  </tr>
</table>

> Effectiveness depends on the actual permissions requested by the application.

---

## Operation Guide

### 1. Add Item
* **Function**: Manually create new UAC bypass registry entries.
* **Usage**: Click the "Add" button and select the target `.exe` file or input a specific command path in the popup dialog. Upon confirmation, the program will create the corresponding registry value under the appropriate path.

### 2. Auto Search
* **Function**: Automatically search for supported games/applications to apply UAC bypass.
* **Currently Supported**:
  * Arknights: Endfield
  * Genshin Impact
  * Honkai: Star Rail
  * Zenless Zone Zero
  * Wuthering Waves
  * Infinity Nikki
  * Honkai Impact 3rd
  * Naraka: Bladepoint
* **Usage**: Click the "Auto Search" button, grant administrator privileges, and verify if the detected files match the support list before executing the addition.

### 3. Batch Delete
* **Function**: Remove bypass settings for selected items from the system registry.
* **Usage**: Select one or more items in the list and click "Delete." The program will invoke Registry APIs to perform the deletion and remove them from the UI list simultaneously.

### 4. Batch Details & Location
* **Function**: View detailed paths and icon information for selected items or locate the physical files.
* **Usage**:
    * **Content Preview**: Hover over an item to display the full file path.
    * **File Location**: Select an item and click "Location." The program will open File Explorer, navigate to the folder, and highlight the physical file.

### 5. Language Switch
* **Function**: Change the interface display language.
* **Usage**: Click the language toggle button. The program will re-map interface strings from JSON language files in real-time, enabling seamless language changes at runtime without needing a restart.

---

## Technical Specifications
* Platform: Windows x64
* Target Framework: .NET 10.0
* UI Framework: WPF (Windows Presentation Foundation)

## Deployment & Download

To ensure the program starts correctly, please choose the appropriate version for your environment:

### Self-contained Version - Recommended
* **Target Audience**: General users or systems where the .NET 10 environment is unverified.
* **Description**: Includes all necessary .NET 10 components. While the file size is larger, it offers the best compatibility.

### Standard Version
* **Target Audience**: Users who have already manually installed the .NET 10 Runtime.
* **Description**: Extremely small file size. Relies on the existing system .NET environment; will fail to launch if the environment version is missing or incorrect.

## Safety Notice
This tool involves write and delete operations within the `HKEY_CURRENT_USER` registry path. Before performing "Batch Delete" or "Add Item," please ensure the target operations align with your intentions.

# In case of any discrepancy, the Traditional Chinese version shall prevail.

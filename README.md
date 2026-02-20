# epi-display-epson_projector
Crestron Essentials Plugin for Epson Projectors (4-Series)

## Features

- Power control (on/off/toggle)
- Video input switching (HDMI, DVI, Computer, Video, LAN, HDBaseT)
- Video mute control
- Video freeze control
- Lamp hours monitoring
- Serial number feedback
- Communication monitoring

## Configuration

### Properties

- **dontUnmuteVideoOnRoute** (bool, default: `false`)
  - When `true`, prevents video from being unmuted during input selection
  - When `false` (default), video is automatically unmuted when selecting an input (preserves current behavior)
  - JSON property: `"dontUnmuteVideoOnRoute"`

### Example Configuration

```json
{
  "key": "roomBBlueDisplay",
  "name": "1044 Blue Display",
  "type": "epsonProjector",
  "group": "display",
  "properties": {
    "warmingTimeMs": 14000,
    "coolingTimeMs": 14000,
    "dontUnmuteVideoOnRoute": true,
    "control": {
      "comParams": {
        "dataBits": 8,
        "softwareHandshake": "None",
        "baudRate": 9600,
        "parity": "None",
        "stopBits": 1,
        "hardwareHandshake": "None",
        "protocol": "RS232"
      },
      "method": "com",
      "controlPortDevKey": "nvxRxBBlueDisplay",
      "controlPortNumber": 1
    }
  }
}
```

## Usage

The plugin implements `IRoutingSinkWithSwitchingWithInputPort` for input switching and provides feedbacks for power status, video mute, freeze, and current input.
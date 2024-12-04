# epi-display-epson_projector



<!-- START Public Methods -->
### Public Methods

- public void Dispatch()
- public void VideoMuteToggle()
- public void VideoMuteOn()
- public void VideoMuteOff()
- public void PowerOn()
- public void PowerOff()
- public void PowerToggle()
- public void ExecuteSwitch(object inputSelector)
- public void StartLensMoveRepeat(eLensFunction func)
- public void StopLensMoveRepeat()
- public void LensFunction(eLensFunction function)
- public void LensPositionRecall(ushort memory)
- public void ProcessResponse(string response)
- public void ProcessResponse(string response)
- public void ProcessResponse(string response)
<!-- END Public Methods -->
<!-- START Join Maps -->
### Join Maps

#### Digitals

| Join | Type (RW) | Description |
| --- | --- | --- |
| 40 | R | VShiftPlus |
| 41 | R | VShiftMinus |
| 42 | R | HShiftPlus |
| 43 | R | HShiftMinus |
| 44 | R | FocusPlus |
| 45 | R | FocusMinus |
| 46 | R | ZoomPlus |
| 47 | R | ZoomMinus |

#### Analogs

| Join | Type (RW) | Description |
| --- | --- | --- |

#### Serials

| Join | Type (RW) | Description |
| --- | --- | --- |
<!-- END Join Maps -->
<!-- START Config Example -->
### Config Example

```json
{
    "key": "GeneratedKey",
    "uid": 1,
    "name": "GeneratedName",
    "type": "epsonProjector",
    "group": "Group",
    "properties": {
        "Control": "SampleValue",
        "Monitor": "SampleValue",
        "EnableBridgeComms": true,
        "TestNewConfigValue": true
    }
}
```
<!-- END Config Example -->

<!-- START Interfaces Implemented -->
### Interfaces Implemented

- IKeyed
- IQueueMessage
- IRoutingSinkWithSwitching
- IHasPowerControlWithFeedback
- IWarmingCooling
- IOnline
- IBasicVideoMuteWithFeedback
- ICommunicationMonitor
- IHasFeedback
<!-- END Interfaces Implemented -->
<!-- START Base Classes -->
### Base Classes

- JoinMapBaseAdvanced
- EventArgs
- EssentialsBridgeableDevice
<!-- END Base Classes -->
<!-- START Supported Types -->
### Supported Types

- epsonProjector
<!-- END Supported Types -->
<!-- START Minimum Essentials Framework Versions -->
### Minimum Essentials Framework Versions

- 1.8.1
<!-- END Minimum Essentials Framework Versions -->

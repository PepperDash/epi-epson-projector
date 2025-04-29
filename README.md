# epi-display-epson_projector
<!-- START Minimum Essentials Framework Versions -->
### Minimum Essentials Framework Versions

- 2.4.7
<!-- END Minimum Essentials Framework Versions -->
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
        "EnableBridgeComms": true
    }
}
```
<!-- END Config Example -->
<!-- START Supported Types -->
### Supported Types

- epsonProjector
<!-- END Supported Types -->
<!-- START Join Maps -->
### Join Maps

#### Digitals

| Join | Type (RW) | Description |
| --- | --- | --- |
| 1 | R | Power Off |
| 2 | R | Power On |
| 11 | R | Warming |
| 12 | R | Cooling |
| 21 | R | Mute Off |
| 22 | R | Mute On |
| 23 | R | Mute Toggle |
| 3 | R | Is Projector |
| 50 | R | Is Online |
| 11 | R | Input Select |
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
| 2 | R | Lamp Hours |
| 5 | R | Lens Position |

#### Serials

| Join | Type (RW) | Description |
| --- | --- | --- |
| 1 | R | Name |
| 2 | R | Status |
| 3 | R | Serial Number |
<!-- END Join Maps -->
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
- EssentialsBridgeableDevice
- EventArgs
<!-- END Base Classes -->
<!-- START Public Methods -->
### Public Methods

- public void ProcessResponse(string response)
- public void Dispatch()
- public void ProcessResponse(string response)
- public void ProcessResponse(string response)
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
<!-- END Public Methods -->
<!-- START Bool Feedbacks -->
### Bool Feedbacks

- VideoMuteIsOn
- PowerIsOnFeedback
- PowerIsOffFeedback
- VideoMuteIsOff
- IsWarmingUpFeedback
- IsCoolingDownFeedback
- IsOnline
<!-- END Bool Feedbacks -->
<!-- START Int Feedbacks -->
### Int Feedbacks

- LampHoursFeedback
- LampHoursFeedback
- CurrentInputValueFeedback
<!-- END Int Feedbacks -->
<!-- START String Feedbacks -->
### String Feedbacks

- SerialNumberFeedback
- SerialNumberFeedback
<!-- END String Feedbacks -->

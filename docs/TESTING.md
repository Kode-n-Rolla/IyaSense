# Testing

IyaSense has undergone **manual functional testing** using a simulated ESP32 in Wokwi and the C#/.NET WPF desktop client. The checks below describe the simulator-based prototype, not validation on physical irrigation hardware.

**Test date:** November 2, 2026  
**Test approach:** Manual end-to-end checks; Wokwi Serial Monitor and WPF observations  
**Environment:** Wokwi ESP32 + DHT22 + potentiometer + relay; MQTT via public HiveMQ broker; Windows WPF client  
**Overall result:** Core functional scenarios passed. Hardware safety and some failure modes remain unverified.

## Functional test matrix

| ID | Scenario | Expected behavior | Result | Evidence |
| --- | --- | --- | --- | --- |
| T01 | Automatic irrigation below 30% soil moisture | Relay turns on when cooldown expires, then turns off after the configured run interval | **PASS** | Serial Monitor: `Pump ON` → `Pump OFF`; telemetry shows pump-state changes |
| T02 | 30-second cooldown | Another run is blocked until cooldown reaches zero | **PASS** | `cooldownRemaining` counts down; `PUMP_START` during cooldown does not trigger `Pump ON` |
| T03 | Automatic mode at 30% or higher soil moisture | No new automatic watering cycle starts | **PASS** | User verified; log shows no new automatic start at 45% after cooldown reached zero |
| T04 | Manual pump start | `PUMP_START` works in Manual mode after cooldown | **PASS** | `Received command: PUMP_START` followed by `Pump ON` |
| T05 | Manual stop | `PUMP_STOP` turns the relay off before the run timeout | **PASS** | `Received command: PUMP_STOP` followed by `Pump OFF` |
| T06 | Switch mode while watering | Active pump stops and cooldown begins | **PASS** | `MODE_AUTO` and `MODE_MANUAL` changes while running are followed by `Pump OFF` |
| T07 | Duplicate start commands | Pump does not restart or reset its run timer while already running | **PASS** | Further `PUMP_START` commands while `pumpRunning: 1` do not produce additional `Pump ON` events |
| T08 | Device stops sending telemetry | WPF shows Offline and disables controls after telemetry timeout | **PASS** | Manually observed in WPF; no dedicated Serial Monitor proof |
| T09 | Device returns | WPF shows Online after fresh telemetry; live values update again | **PASS** | Manually observed in WPF; no dedicated Serial Monitor proof |
| T10 | MQTT broker unavailable at startup | Local automatic irrigation continues despite publish failures | **PASS (limited)** | Tested with a loopback/unavailable broker; `Publish failed!` while `Pump ON` and `Pump OFF` still occur |

## Selected serial-log evidence

The following abbreviated excerpts come from the final Wokwi functional test session (November 2, 2026).

### Automatic pump cycle

```text
{"soilMoisture":13,"pumpRunning":false,"autoMode":true,"cooldownRemaining":1804}
Pump ON
{"soilMoisture":13,"pumpRunning":true,"autoMode":true,"cooldownRemaining":0}
Pump OFF
{"soilMoisture":13,"pumpRunning":false,"autoMode":true,"cooldownRemaining":27804}
```

### Manual stop and cooldown

```text
Received command: PUMP_START
Pump ON
Received command: PUMP_START
Pump running: 1
Received command: PUMP_STOP
Pump OFF
{"soilMoisture":45,"pumpRunning":false,"autoMode":false,"cooldownRemaining":28561}
```

### Switching modes during an active run

```text
Received command: PUMP_START
Pump ON
Received command: MODE_AUTO
Pump running: 1
Pump OFF
{"soilMoisture":3,"pumpRunning":false,"autoMode":true,"cooldownRemaining":27629}
```

These are excerpts with unrelated telemetry lines omitted; they demonstrate event order, not precise wall-clock timing. The configured five-second run interval is a software limit, not an independently measured hardware cutoff.

## Known limitations and unverified cases

- **Simulator only.** No physical soil sensor, relay-driven pump, power electronics, or real-world watering setup has been tested.
- **No hard real-time safety guarantee.** Network calls (including MQTT connect/loop) can block the ESP32 loop; the five-second timer is not independent hardware protection.
- **Unexpected network loss is not fully characterized.** Normal connect/disconnect, device telemetry timeout, and startup without a broker were checked, but sustained TCP stalls and abrupt link loss during an active pump run were not exhaustively tested.
- **No automated test suite or CI results.** All results here are manual observations; the badges in README refer to these manual functional tests.
- **Command rejection feedback is limited.** A manual start during cooldown is ignored by the controller. The WPF application shows the cooldown value but does not display a dedicated rejection message.
- **Telemetry updates every ~3 seconds.** UI state can lag behind relay transitions, and the app's Online/Offline indication is based on freshness of telemetry rather than direct hardware attestation.
- **Public test broker.** The current topic and client identifiers are demonstration values. Use unique topics/IDs for separate instances; the public broker is not authenticated production infrastructure.

## Release readiness

**Core simulation functionality: manually tested.** UI polish, documentation updates, and final packaging remain. IyaSense is **not ready for unattended physical irrigation** without additional safety design and hardware validation.

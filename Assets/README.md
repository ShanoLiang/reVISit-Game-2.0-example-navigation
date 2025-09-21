# WebGL JSON Data Summary

This document describes the JSON posts sent from Unity to WebGL and the expected structure for each type.

---

## 1. Route Completion Data
**Type:** `RouteCompletionData`

```
{
  "routes": [
    { "routeIndex": 0, "isComplete": true, "timeSpent": 87.2 },
    { "routeIndex": 1, "isComplete": true, "timeSpent": 90.0 }
    // ...more routes
  ]
}
```

---

## 2. Map Open Events Data
**Type:** `M_Map`

```
{
  "events": [
    { "openTime": 12.5, "duration": 3.2 },
    { "openTime": 25.7, "duration": 2.8 }
    // ...more events
  ]
}
```

---

## 3. Player Distance Data
**Type:** `PlayerDistance`

```
{
  "distance": 123.45
}
```

---

## 4. Test Summary Meta Data
**Type:** `TestSummaryMetaData`

```
{
  "sceneName": "SampleScene",
  "assistanceType": "Strong",
  "totalMapReviewTime": 12.3,
  "totalDistanceTravelled": 123.45,
  "totalCompletionTime": 180.0
}
```

---

## Assistance Type from URL

If running in WebGL, the game manager will parse the URL for the parameter `assistance=` to determine the assistance type:
- `assistance=0` → None
- `assistance=1` → Moderate
- `assistance=2` → Strong

**Example URL:**
```
https://yourgameurl.com/?assistance=2
```

This will set the assistance type to `Strong` at runtime.

**On the WebGL side, catch these types and parse the JSON payloads accordingly.**

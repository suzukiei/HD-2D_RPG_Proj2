using System.Collections.Generic;
using UnityEngine;

public sealed class CameraTurnZoneResolver : MonoBehaviour
{
    [SerializeField] private FieldCameraTurnController turnController;

    private readonly List<CameraTurnZone> _activeZones = new List<CameraTurnZone>();

    public void EnterZone(CameraTurnZone zone)
    {
        if (zone == null) return;

        if (!_activeZones.Contains(zone))
        {
            _activeZones.Add(zone);
        }

        RefreshCameraTurn();
    }

    public void ExitZone(CameraTurnZone zone)
    {
        if (zone == null) return;

        _activeZones.Remove(zone);
        RefreshCameraTurn();
    }

    private void RefreshCameraTurn()
    {
        if (turnController == null) return;

        CameraTurnZone bestZone = GetBestZone();

        if (bestZone == null)
        {
            turnController.ReturnToDefault();
            return;
        }

        turnController.SetDirection(bestZone.Direction);
    }

    private CameraTurnZone GetBestZone()
    {
        if (_activeZones.Count == 0) return null;

        CameraTurnZone best = null;

        for (int i = 0; i < _activeZones.Count; i++)
        {
            CameraTurnZone zone = _activeZones[i];
            if (zone == null) continue;

            if (best == null || zone.Priority > best.Priority)
            {
                best = zone;
            }
        }

        return best;
    }
}
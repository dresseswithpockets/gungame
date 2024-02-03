using Godot;
using System;
using Godot.Collections;
using GunGame;

[Tool]
public partial class LightSpot : SpotLight3D
{
    [Export] public Dictionary properties;
    [Export] public bool flicker;
    [Export] public float baseLightEnergy;

    private static readonly float DefaultRadius = QodotMath.TbUnitsToGodot(5f);
    private static readonly string FlickerString = "mmamammmmammamamaaamammma";
    private const float FlickerTime = 0.1f;
    private float _flickerTimer;
    private int _flickerIndex;

    public void UpdateProperties(Node3D _)
    {
        // Spot settings
        SpotRange = properties.GetOrDefault("spot_range", DefaultRadius);
        SpotRange = QodotMath.TbUnitsToGodot(SpotRange);
        SpotAngle = properties.GetOrDefault("spot_angle", 45f);
        SpotAttenuation = properties.GetOrDefault("spot_attenuation", 1f);
        SpotAngleAttenuation = properties.GetOrDefault("spot_angle_attenuation", 1f);

        // General Light settings
        LightColor = properties.GetOrDefault("color", Colors.White);
        baseLightEnergy = properties.GetOrDefault("energy", 1f);
        LightEnergy = baseLightEnergy;
        LightIndirectEnergy = baseLightEnergy;
        var isShortLived = properties.GetOrDefault("short_lived", false);
        LightVolumetricFogEnergy = isShortLived ? 0f : baseLightEnergy;
        LightBakeMode = (BakeMode)properties.GetOrDefault("bake_mode", (int)BakeMode.Dynamic);

        ShadowEnabled = properties.GetOrDefault("shadows", false);

        flicker = properties.GetOrDefault("flicker", false);
    }

    public override void _Process(double delta)
    {
        if (!flicker) return;
        _flickerTimer += (float)delta;
        var changed = false;
        while (_flickerTimer >= FlickerTime)
        {
            _flickerTimer -= FlickerTime;
            _flickerIndex++;
            if (_flickerIndex == FlickerString.Length)
                _flickerIndex = 0;
            changed = true;
        }

        if (changed)
            LightEnergy = CalculateEnergy();
    }

    private float CalculateEnergy() => baseLightEnergy * (float)(FlickerString[_flickerIndex] - 'a') / (float)'m';
}
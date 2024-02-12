using Godot;
using Godot.Collections;
using GunGame;
using Environment = Godot.Environment;

[Tool]
public partial class EnvEnvironment : Node
{
    [Export] public Dictionary properties;
    [Export] public Environment environment;
    [Export] public EnvPostprocess target;

    public void UpdateProperties(Node3D qodotMap)
    {
        if (!Engine.IsEditorHint())
            return;

        SetupTargetPostProcess(qodotMap);

        environment = new Environment();
        environment.AdjustmentEnabled = properties.GetOrDefault("adjustment_enabled", false);
        // NOTE: adjustment_color_correction is ignored
        environment.AdjustmentBrightness = properties.GetOrDefault("adjustment_brightness", 1.0f);
        environment.AdjustmentContrast = properties.GetOrDefault("adjustment_contrast", 1.0f);
        environment.AdjustmentSaturation = properties.GetOrDefault("adjustment_saturation", 1.0f);

        environment.AmbientLightSource = (Environment.AmbientSource)properties.GetOrDefault("ambient_light_source", 0);
        environment.AmbientLightColor = properties.GetOrDefault("ambient_light_color", Colors.Black);
        environment.AmbientLightEnergy = properties.GetOrDefault("ambient_light_energy", 1.0f);
        environment.AmbientLightSkyContribution = properties.GetOrDefault("ambient_light_sky_contribution", 1.0f);

        // TODO: support other background modes, for now only sky is supported
        environment.BackgroundMode = Environment.BGMode.Sky;
        environment.BackgroundEnergyMultiplier = properties.GetOrDefault("background_energy_multiplier", 1.0f);

        environment.FogEnabled = properties.GetOrDefault("fog_enabled", false);
        environment.FogAerialPerspective = properties.GetOrDefault("fog_aerial_perspective", 0.0f);
        environment.FogDensity = properties.GetOrDefault("fog_density", 0.01f);
        environment.FogHeight = properties.GetOrDefault("fog_height", 0.0f);
        environment.FogHeightDensity = properties.GetOrDefault("fog_height_density", 0.0f);
        environment.FogLightColor = properties.GetOrDefault("fog_light_color", environment.FogLightColor);
        environment.FogLightEnergy = properties.GetOrDefault("fog_light_energy", 1.0f);
        environment.FogSkyAffect = properties.GetOrDefault("fog_sky_affect", 1.0f);
        environment.FogSunScatter = properties.GetOrDefault("fog_sun_scatter", 0.0f);

        // TODO: support glow

        environment.ReflectedLightSource =
            (Environment.ReflectionSource)properties.GetOrDefault("reflected_light_source", 0);

        // TODO: add sky properties
        var skyType = (SkyType)properties.GetOrDefault("sky", (int)SkyType.Procedural);
        environment.Sky = new Sky();
        if (skyType == 0)
        {
            environment.Sky.SkyMaterial = new ProceduralSkyMaterial();
        }
        else
        {
            var panoramaMaterial = new PanoramaSkyMaterial();
            environment.Sky.SkyMaterial = panoramaMaterial;
            
            var skyTextureName = properties.GetOrDefault("sky_panorama", "");
            if (string.IsNullOrWhiteSpace(skyTextureName))
                GD.PushWarning($"'{Name}' has no sky_panorama despite selecting Panorama sky mode.");
            else
            {
                if (!skyTextureName.StartsWith("res://"))
                    skyTextureName = $"res://{skyTextureName}";

                if (!ResourceLoader.Exists(skyTextureName, "Texture2D"))
                    GD.PushWarning($"'{Name}': no Texture2D at sky_panorama path: '{skyTextureName}'.");
                else
                    panoramaMaterial.Panorama = GD.Load<Texture2D>(skyTextureName);
            }
        }

        environment.SsaoEnabled = properties.GetOrDefault("ssao_enabled", false);
        environment.SsaoAOChannelAffect = properties.GetOrDefault("ssao_ao_channel_affect", 0.0f);
        environment.SsaoDetail = properties.GetOrDefault("ssao_detail", 0.5f);
        environment.SsaoHorizon = properties.GetOrDefault("ssao_horizon", 0.06f);
        environment.SsaoIntensity = properties.GetOrDefault("ssao_intensity", 2.0f);
        environment.SsaoLightAffect = properties.GetOrDefault("ssao_light_affect", 0.0f);
        environment.SsaoPower = properties.GetOrDefault("ssao_power", 1.5f);
        environment.SsaoRadius = properties.GetOrDefault("ssao_radius", 1.0f);
        environment.SsaoSharpness = properties.GetOrDefault("ssao_sharpness", 0.98f);

        environment.SsilEnabled = properties.GetOrDefault("ssil_enabled", false);
        environment.SsilIntensity = properties.GetOrDefault("ssil_intensity", 1.0f);
        environment.SsilNormalRejection = properties.GetOrDefault("ssil_normal_rejection", 1.0f);
        environment.SsilRadius = properties.GetOrDefault("ssil_radius", 5.0f);
        environment.SsilSharpness = properties.GetOrDefault("ssil_sharpness", 0.98f);

        environment.SsrEnabled = properties.GetOrDefault("ssr_enabled", false);
        environment.SsrDepthTolerance = properties.GetOrDefault("ssr_depth_tolerance", 0.2f);
        environment.SsrFadeIn = properties.GetOrDefault("ssr_fade_in", 0.15f);
        environment.SsrFadeOut = properties.GetOrDefault("ssr_fade_out", 2.0f);
        environment.SsrMaxSteps = properties.GetOrDefault("ssr_max_steps", 64);

        environment.TonemapExposure = properties.GetOrDefault("tonemap_exposure", 1.0f);
        environment.TonemapMode = (Environment.ToneMapper)properties.GetOrDefault("tonemap_mode", 0);
        environment.TonemapWhite = properties.GetOrDefault("tonemap_white", 1.0f);

        environment.VolumetricFogEnabled = properties.GetOrDefault("volumetric_fog_enabled", false);
        environment.VolumetricFogAlbedo = properties.GetOrDefault("volumetric_fog_albedo", Colors.White);
        environment.VolumetricFogAmbientInject = properties.GetOrDefault("volumetric_fog_ambient_inject", 0.0f);
        environment.VolumetricFogAnisotropy = properties.GetOrDefault("volumetric_fog_anisotropy", 0.2f);
        environment.VolumetricFogDensity = properties.GetOrDefault("volumetric_fog_density", 0.05f);
        environment.VolumetricFogDetailSpread = properties.GetOrDefault("volumetric_fog_detail_spread", 2.0f);
        environment.VolumetricFogEmission = properties.GetOrDefault("volumetric_fog_emission", Colors.Black);
        environment.VolumetricFogEmissionEnergy = properties.GetOrDefault("volumetric_fog_emission_energy", 1.0f);
        environment.VolumetricFogGIInject = properties.GetOrDefault("volumetric_fog_gi_inject", 1.0f);
        environment.VolumetricFogLength = properties.GetOrDefault("volumetric_fog_length", 64.0f);
        environment.FogSkyAffect = properties.GetOrDefault("volumetric_fog_sky_affect", 1.0f);

        environment.VolumetricFogTemporalReprojectionEnabled =
            properties.GetOrDefault("volumetric_fog_temporal_reprojection_enabled", false);
        environment.VolumetricFogTemporalReprojectionAmount =
            properties.GetOrDefault("volumetric_fog_temporal_reprojection_amount", 0.9f);
    }

    private void SetupTargetPostProcess(GodotObject qodotMap)
    {
        var targetPostprocessName = properties.GetOrDefault("target", (string)null);
        if (string.IsNullOrEmpty(targetPostprocessName))
        {
            GD.PushWarning($"'{Name}' has no target env_postprocess, so it will not function.");
            return;
        }

        var targetNodes = qodotMap.Call("get_nodes_by_targetname", targetPostprocessName).AsGodotObjectArray<Node>();
        switch (targetNodes.Length)
        {
            case 0:
                GD.PushWarning(
                    $"'{Name}' targets '{targetPostprocessName}', but there are no entities with that name, so it will not function");
                return;
            case > 1:
                GD.PushWarning($"'{Name}' targets multiple entities named '{targetPostprocessName}'. Will only link to the first one.");
                break;
        }

        var targetNode = targetNodes[0];
        if (targetNode is not EnvPostprocess targetPostprocess)
        {
            GD.PushError($"'{Name}' must target a EnvPostprocess-derived entity, but it targets '{targetPostprocessName}', which doesn't derive from EnvPostprocess.");
            return;
        }

        target = targetPostprocess;
    }

    public void Use(Node3D _)
    {
        if (target == null || !IsInstanceValid(target))
            return;

        target.Environment = environment;
    }
}

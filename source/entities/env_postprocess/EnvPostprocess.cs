using Godot;
using Godot.Collections;
using GunGame;
using Environment = Godot.Environment;

[Tool]
public partial class EnvPostprocess : WorldEnvironment
{
    [Export] public Dictionary properties;
    [Export] public bool dirty;
    [Export] public Environment defaultEnvironment;

    public void UpdateProperties(Node3D qodotMap)
    {
        if (!Engine.IsEditorHint())
            return;

        // TODO: do we care about multiple env_postprocesses in the scene? maybe if theyre active at once

        defaultEnvironment = new Environment();
        defaultEnvironment.AdjustmentEnabled = properties.GetOrDefault("adjustment_enabled", false);
        // NOTE: adjustment_color_correction is ignored
        defaultEnvironment.AdjustmentBrightness = properties.GetOrDefault("adjustment_brightness", 1.0f);
        defaultEnvironment.AdjustmentContrast = properties.GetOrDefault("adjustment_contrast", 1.0f);
        defaultEnvironment.AdjustmentSaturation = properties.GetOrDefault("adjustment_saturation", 1.0f);

        defaultEnvironment.AmbientLightSource = (Environment.AmbientSource)properties.GetOrDefault("ambient_light_source", 0);
        defaultEnvironment.AmbientLightColor = properties.GetOrDefault("ambient_light_color", Colors.Black);
        defaultEnvironment.AmbientLightEnergy = properties.GetOrDefault("ambient_light_energy", 1.0f);
        defaultEnvironment.AmbientLightSkyContribution = properties.GetOrDefault("ambient_light_sky_contribution", 1.0f);

        // TODO: support other background modes, for now only sky is supported
        defaultEnvironment.BackgroundMode = Environment.BGMode.Sky;
        defaultEnvironment.BackgroundEnergyMultiplier = properties.GetOrDefault("background_energy_multiplier", 1.0f);

        defaultEnvironment.FogEnabled = properties.GetOrDefault("fog_enabled", false);
        defaultEnvironment.FogAerialPerspective = properties.GetOrDefault("fog_aerial_perspective", 0.0f);
        defaultEnvironment.FogDensity = properties.GetOrDefault("fog_density", 0.01f);
        defaultEnvironment.FogHeight = properties.GetOrDefault("fog_height", 0.0f);
        defaultEnvironment.FogHeightDensity = properties.GetOrDefault("fog_height_density", 0.0f);
        defaultEnvironment.FogLightColor = properties.GetOrDefault("fog_light_color", defaultEnvironment.FogLightColor);
        defaultEnvironment.FogLightEnergy = properties.GetOrDefault("fog_light_energy", 1.0f);
        defaultEnvironment.FogSkyAffect = properties.GetOrDefault("fog_sky_affect", 1.0f);
        defaultEnvironment.FogSunScatter = properties.GetOrDefault("fog_sun_scatter", 0.0f);

        // TODO: support glow

        defaultEnvironment.ReflectedLightSource =
            (Environment.ReflectionSource)properties.GetOrDefault("reflected_light_source", 0);

        // TODO: add sky properties
        var skyType = (SkyType)properties.GetOrDefault("sky", (int)SkyType.Procedural);
        defaultEnvironment.Sky = new Sky();
        if (skyType == 0)
        {
            defaultEnvironment.Sky.SkyMaterial = new ProceduralSkyMaterial();
        }
        else
        {
            var panoramaMaterial = new PanoramaSkyMaterial();
            defaultEnvironment.Sky.SkyMaterial = panoramaMaterial;
            
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

        defaultEnvironment.SsaoEnabled = properties.GetOrDefault("ssao_enabled", false);
        defaultEnvironment.SsaoAOChannelAffect = properties.GetOrDefault("ssao_ao_channel_affect", 0.0f);
        defaultEnvironment.SsaoDetail = properties.GetOrDefault("ssao_detail", 0.5f);
        defaultEnvironment.SsaoHorizon = properties.GetOrDefault("ssao_horizon", 0.06f);
        defaultEnvironment.SsaoIntensity = properties.GetOrDefault("ssao_intensity", 2.0f);
        defaultEnvironment.SsaoLightAffect = properties.GetOrDefault("ssao_light_affect", 0.0f);
        defaultEnvironment.SsaoPower = properties.GetOrDefault("ssao_power", 1.5f);
        defaultEnvironment.SsaoRadius = properties.GetOrDefault("ssao_radius", 1.0f);
        defaultEnvironment.SsaoSharpness = properties.GetOrDefault("ssao_sharpness", 0.98f);

        defaultEnvironment.SsilEnabled = properties.GetOrDefault("ssil_enabled", false);
        defaultEnvironment.SsilIntensity = properties.GetOrDefault("ssil_intensity", 1.0f);
        defaultEnvironment.SsilNormalRejection = properties.GetOrDefault("ssil_normal_rejection", 1.0f);
        defaultEnvironment.SsilRadius = properties.GetOrDefault("ssil_radius", 5.0f);
        defaultEnvironment.SsilSharpness = properties.GetOrDefault("ssil_sharpness", 0.98f);

        defaultEnvironment.SsrEnabled = properties.GetOrDefault("ssr_enabled", false);
        defaultEnvironment.SsrDepthTolerance = properties.GetOrDefault("ssr_depth_tolerance", 0.2f);
        defaultEnvironment.SsrFadeIn = properties.GetOrDefault("ssr_fade_in", 0.15f);
        defaultEnvironment.SsrFadeOut = properties.GetOrDefault("ssr_fade_out", 2.0f);
        defaultEnvironment.SsrMaxSteps = properties.GetOrDefault("ssr_max_steps", 64);

        defaultEnvironment.TonemapExposure = properties.GetOrDefault("tonemap_exposure", 1.0f);
        defaultEnvironment.TonemapMode = (Environment.ToneMapper)properties.GetOrDefault("tonemap_mode", 0);
        defaultEnvironment.TonemapWhite = properties.GetOrDefault("tonemap_white", 1.0f);

        defaultEnvironment.VolumetricFogEnabled = properties.GetOrDefault("volumetric_fog_enabled", false);
        defaultEnvironment.VolumetricFogAlbedo = properties.GetOrDefault("volumetric_fog_albedo", Colors.White);
        defaultEnvironment.VolumetricFogAmbientInject = properties.GetOrDefault("volumetric_fog_ambient_inject", 0.0f);
        defaultEnvironment.VolumetricFogAnisotropy = properties.GetOrDefault("volumetric_fog_anisotropy", 0.2f);
        defaultEnvironment.VolumetricFogDensity = properties.GetOrDefault("volumetric_fog_density", 0.05f);
        defaultEnvironment.VolumetricFogDetailSpread = properties.GetOrDefault("volumetric_fog_detail_spread", 2.0f);
        defaultEnvironment.VolumetricFogEmission = properties.GetOrDefault("volumetric_fog_emission", Colors.Black);
        defaultEnvironment.VolumetricFogEmissionEnergy = properties.GetOrDefault("volumetric_fog_emission_energy", 1.0f);
        defaultEnvironment.VolumetricFogGIInject = properties.GetOrDefault("volumetric_fog_gi_inject", 1.0f);
        defaultEnvironment.VolumetricFogLength = properties.GetOrDefault("volumetric_fog_length", 64.0f);
        defaultEnvironment.FogSkyAffect = properties.GetOrDefault("volumetric_fog_sky_affect", 1.0f);

        defaultEnvironment.VolumetricFogTemporalReprojectionEnabled =
            properties.GetOrDefault("volumetric_fog_temporal_reprojection_enabled", false);
        defaultEnvironment.VolumetricFogTemporalReprojectionAmount =
            properties.GetOrDefault("volumetric_fog_temporal_reprojection_amount", 0.9f);

        Environment = defaultEnvironment;
    }

    public void Use(Node3D _)
    {
        // just reset to the default environment
        Environment = defaultEnvironment;
    }
}

public enum SkyType
{
    Procedural = 0,
    Panorama = 1,
}

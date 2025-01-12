using fluXis.Game.Configuration;
using fluXis.Game.Overlay.Settings.UI;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;

namespace fluXis.Game.Overlay.Settings.Sections.Audio;

public partial class AudioVolumeSection : SettingsSubSection
{
    public override string Title => "Volume";

    [BackgroundDependencyLoader]
    private void load(AudioManager audio)
    {
        AddRange(new Drawable[]
        {
            new SettingsSlider<double>
            {
                Label = "Master Volume",
                Bindable = audio.Volume,
                DisplayAsPercentage = true
            },
            new SettingsSlider<double>
            {
                Label = "Music Volume",
                Bindable = audio.VolumeTrack,
                DisplayAsPercentage = true
            },
            new SettingsSlider<double>
            {
                Label = "Effect Volume",
                Bindable = audio.VolumeSample,
                DisplayAsPercentage = true
            },
            new SettingsSlider<double>
            {
                Label = "Hit Sound Volume",
                Bindable = Config.GetBindable<double>(FluXisSetting.HitSoundVolume),
                DisplayAsPercentage = true
            }
        });
    }
}

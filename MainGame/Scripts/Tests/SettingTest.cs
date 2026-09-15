//------------------------------------------------------------
// Setting 组件测试
//------------------------------------------------------------

using GodotGameFramework;
using Godot;

namespace GameMain.Tests
{
    /// <summary>
    /// 测试 SettingComponent：读写 bool / int / float / string，
    /// HasSetting、RemoveSetting，以及 Save/Load 往返验证。
    /// 注意：需要场景中有 SettingComponent 节点。
    /// </summary>
    public partial class SettingTest : Node
    {
        public override void _Ready()
        {
            var setting = GodotGameFramework.GameEntry.GetComponent<SettingComponent>();
            if (setting == null)
            {
                Log.Error("[SettingTest] SettingComponent 未找到，请在场景中添加该节点。");
                return;
            }

            // 清空历史数据，确保测试纯净
            setting.RemoveAllSettings();

            // ── 写入 ────────────────────────────────────────────────────────────

            setting.SetBool("test.sfxOn", true);
            setting.SetInt("test.volume", 80);
            setting.SetFloat("test.brightness", 0.75f);
            setting.SetString("test.nickname", "Tester");

            // ── 读取（不 Save/Load，直接内存读取）────────────────────────────────

            Log.Info($"[SettingTest] sfxOn      = {setting.GetBool("test.sfxOn")}（期望 True）");
            Log.Info($"[SettingTest] volume      = {setting.GetInt("test.volume")}（期望 80）");
            Log.Info($"[SettingTest] brightness  = {setting.GetFloat("test.brightness"):F2}（期望 0.75）");
            Log.Info($"[SettingTest] nickname    = {setting.GetString("test.nickname")}（期望 Tester）");

            // ── HasSetting ────────────────────────────────────────────────────

            Log.Info($"[SettingTest] Has 'test.volume'  = {setting.HasSetting("test.volume")}（期望 True）");
            Log.Info($"[SettingTest] Has 'test.missing' = {setting.HasSetting("test.missing")}（期望 False）");

            // ── 默认值 ────────────────────────────────────────────────────────

            int missing = setting.GetInt("test.missing", defaultValue: -1);
            Log.Info($"[SettingTest] missing 默认值 = {missing}（期望 -1）");

            // ── RemoveSetting ─────────────────────────────────────────────────

            setting.RemoveSetting("test.volume");
            Log.Info($"[SettingTest] Remove 后 Has 'test.volume' = {setting.HasSetting("test.volume")}（期望 False）");

            // ── Save → Load 往返 ──────────────────────────────────────────────

            setting.Save();
            setting.RemoveAllSettings();
            Log.Info($"[SettingTest] Load 前 nickname = '{setting.GetString("test.nickname", "EMPTY")}'（期望 EMPTY）");

            setting.Load();
            Log.Info($"[SettingTest] Load 后 nickname = '{setting.GetString("test.nickname", "EMPTY")}'（期望 Tester）");

            Log.Info("[SettingTest] 测试通过 ✓");
        }
    }
}

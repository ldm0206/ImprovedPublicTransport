using System;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using UnityEngine;
using UIUtils = ImprovedPublicTransport.Util.UIUtils;
using Utils = ImprovedPublicTransport.Util.Utils;

namespace ImprovedPublicTransport.UI
{
    /// <summary>
    /// Shows a small panel while the city treasury is negative (e.g. after a save whose
    /// economy was damaged by the taxi fare overflow), offering a one-click rescue that
    /// sets the treasury to ₡100,000. Lives on IptGameObject, so it is per-level-session;
    /// the close button dismisses it until the next load.
    /// </summary>
    public class NegativeCashRescue : MonoBehaviour
    {
        private const long TargetCash = 10000000L; // ₡100,000.00 in fare units

        private UIPanel _panel;
        private UILabel _moneyLabel;
        private bool _dismissed;
        private float _checkTimer;

        private void Update()
        {
            _checkTimer += Time.deltaTime;
            if (_checkTimer < 1f)
            {
                return;
            }
            _checkTimer = 0f;

            bool negative = !_dismissed &&
                            Singleton<EconomyManager>.exists &&
                            Singleton<EconomyManager>.instance.InternalCashAmount < 0;
            if (!negative)
            {
                if (_panel != null && _panel.isVisible)
                {
                    _panel.Hide();
                }
                return;
            }

            EnsurePanel();
            if (_panel == null)
            {
                return;
            }

            long cash = Singleton<EconomyManager>.instance.InternalCashAmount;
            _moneyLabel.text = ((double)cash / 100.0).ToString(
                Locale.Get("MONEY_FORMAT"), LocaleManager.cultureInfo);
            if (!_panel.isVisible)
            {
                _panel.Show();
                _panel.BringToFront();
            }
        }

        private void EnsurePanel()
        {
            if (_panel != null)
            {
                return;
            }
            var view = UIView.GetAView();
            if (view == null)
            {
                return;
            }

            const float panelWidth = 380f;
            const float panelHeight = 130f;

            // UIView shadows the generic AddUIComponent with a non-generic one.
            _panel = (UIPanel)view.AddUIComponent(typeof(UIPanel));
            _panel.name = "IptNegativeCashRescue";
            _panel.backgroundSprite = "UnlockingPanel2";
            _panel.canFocus = true;
            _panel.isInteractive = true;
            _panel.size = new Vector2(panelWidth, panelHeight);
            _panel.relativePosition = new Vector2(
                Mathf.Floor((view.fixedWidth - panelWidth) / 2f),
                Mathf.Floor((view.fixedHeight - panelHeight) / 3f));
            _panel.Hide();

            var title = _panel.AddUIComponent<UILabel>();
            title.font = UIUtils.Font;
            title.text = Localization.Get("NEGATIVE_CASH_RESCUE_TITLE");
            title.textAlignment = UIHorizontalAlignment.Center;
            title.textScale = 1.1f;
            title.autoSize = false;
            title.size = new Vector2(panelWidth, 24f);
            title.relativePosition = new Vector3(0f, 12f);

            _moneyLabel = _panel.AddUIComponent<UILabel>();
            _moneyLabel.font = UIUtils.Font;
            _moneyLabel.textAlignment = UIHorizontalAlignment.Center;
            _moneyLabel.autoSize = false;
            _moneyLabel.size = new Vector2(panelWidth, 20f);
            _moneyLabel.relativePosition = new Vector3(0f, 44f);

            var button = UIUtils.CreateButton(_panel);
            button.text = Localization.Get("NEGATIVE_CASH_RESCUE_BUTTON");
            button.size = new Vector2(220f, 36f);
            button.relativePosition = new Vector3((panelWidth - 220f) / 2f, 76f);
            button.eventClick += (component, eventParam) =>
            {
                SetCashTo(TargetCash);
                _panel.Hide();
            };

            var closeButton = _panel.AddUIComponent<UIButton>();
            closeButton.relativePosition = new Vector2(panelWidth - 35f, 2f);
            closeButton.normalBgSprite = "buttonclose";
            closeButton.hoveredBgSprite = "buttonclosehover";
            closeButton.pressedBgSprite = "buttonclosepressed";
            closeButton.eventClick += (component, eventParam) =>
            {
                _dismissed = true;
                _panel.Hide();
            };
        }

        private static void SetCashTo(long targetUnits)
        {
            // AddResource mutates economy state — run it on the simulation thread.
            SimulationManager.instance.AddAction(() =>
            {
                try
                {
                    long diff = targetUnits - Singleton<EconomyManager>.instance.InternalCashAmount;
                    // diff is always positive here (treasury is negative), but the damage may
                    // exceed int range, so top up in chunks.
                    while (diff > int.MaxValue)
                    {
                        Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.BailoutAmount,
                            int.MaxValue, ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Level.None);
                        diff -= int.MaxValue;
                    }
                    if (diff > 0)
                    {
                        Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.BailoutAmount,
                            (int)diff, ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Level.None);
                    }
                    Utils.Log($"NegativeCashRescue: treasury set to {targetUnits / 100.0:F2}.");
                }
                catch (Exception ex)
                {
                    Utils.LogError($"NegativeCashRescue: failed to set treasury: {ex.Message}");
                }
            });
        }
    }
}

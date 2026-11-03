using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public static HUDController Instance { get; private set; }

    public TMP_Text playerIdentityText;
    public TMP_Text playerStatsText;
    public Image weapon1Icon;
    public Image weapon2Icon;
    public Color weaponActiveColor = Color.white;
    public Color weaponInactiveColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);

    public Image healthFill;

    public TMP_Text killFeedText;

    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public Button backToMenuButton;

    private PlayerHealth _localHealth;
    private SessionPlayer _localSession;

    private static TMP_FontAsset s_defaultFont;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        EnsureDefaultFont();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
        RefreshAllTexts();
    }

    public void RefreshAllTexts()
    {
        if (playerIdentityText != null) playerIdentityText.text = "Eres: Jugador ?";
        if (playerStatsText != null) playerStatsText.text = "Vida: 100/100\nEscudos: 0/5\nKills: 0/5";
        if (killFeedText != null) killFeedText.text = "";
        if (healthFill != null) healthFill.fillAmount = 1f;
        if (weapon1Icon != null) weapon1Icon.color = weaponActiveColor;
        if (weapon2Icon != null) weapon2Icon.color = weaponInactiveColor;
    }

    public void ActivateForGame()
    {
        BuildIfMissing();

        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        GameObject menu = GameObject.Find("Menu");
        if (menu != null) menu.SetActive(false);
    }

    public void DeactivateToMenu()
    {
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
        if (gameObject.activeSelf) gameObject.SetActive(false);

        GameObject menu = GameObject.Find("Menu");
        if (menu != null) menu.SetActive(true);
    }

    public void RegisterHealth(PlayerHealth h)
    {
        _localHealth = h;
    }

    public void UnregisterHealth(PlayerHealth h)
    {
        if (_localHealth == h) _localHealth = null;
    }

    public void RegisterSessionPlayer(SessionPlayer sp)
    {
        if (sp == null) return;
        if (!sp.IsLocalPlayer) return;
        _localSession = sp;
    }

    public void UnregisterSessionPlayer(SessionPlayer sp)
    {
        if (_localSession == sp) _localSession = null;
    }

    public void OnWeaponChanged(int newWeapon)
    {
        if (weapon1Icon != null) weapon1Icon.color = newWeapon == 1 ? weaponActiveColor : weaponInactiveColor;
        if (weapon2Icon != null) weapon2Icon.color = newWeapon == 2 ? weaponActiveColor : weaponInactiveColor;
    }

    private struct KillFeedEntry { public string message; public float until; }
    private readonly List<KillFeedEntry> _killFeedEntries = new List<KillFeedEntry>();
    private const float KillFeedLifetime = 5f;

    public void AddKillFeedEntry(string message)
    {
        _killFeedEntries.Add(new KillFeedEntry { message = message, until = Time.unscaledTime + KillFeedLifetime });
        if (_killFeedEntries.Count > 12) _killFeedEntries.RemoveAt(0);
        RefreshKillFeed();
    }

    private void RefreshKillFeed()
    {
        if (killFeedText == null) return;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < _killFeedEntries.Count; i++)
        {
            sb.AppendLine(_killFeedEntries[i].message);
        }
        killFeedText.text = sb.ToString();
    }

    private void Update()
    {
        if (_killFeedEntries.Count > 0)
        {
            bool removed = false;
            for (int i = _killFeedEntries.Count - 1; i >= 0; i--)
            {
                if (Time.unscaledTime > _killFeedEntries[i].until)
                {
                    _killFeedEntries.RemoveAt(i);
                    removed = true;
                }
            }
            if (removed) RefreshKillFeed();
        }

        if (_localHealth != null)
        {
            if (healthFill != null)
            {
                float pct = Mathf.Clamp01((float)_localHealth.CurrentHealth / _localHealth.MaxHealth);
                healthFill.fillAmount = pct;
            }
        }

        if (playerIdentityText != null && _localSession != null)
            playerIdentityText.text = $"Eres: Jugador {_localSession.PlayerNumber}";

        refreshPlayerStats();

        if (_localSession != null && _localSession.IsMatchEnded && MatchController.Instance != null)
        {
            bool localWon = MatchController.Instance.WinnerPlayerNumber == _localSession.PlayerNumber;
            if (victoryPanel != null) victoryPanel.SetActive(localWon);
            if (defeatPanel != null) defeatPanel.SetActive(!localWon);
        }
    }

    private void refreshPlayerStats()
    {
        if (playerStatsText == null) return;
        if (_localHealth == null || _localSession == null) return;

        string health = "<color=" + getHealthColor(_localHealth.CurrentHealth) + ">Vida: " + _localHealth.CurrentHealth + "/" + _localHealth.MaxHealth + "</color>";
        string shields = "<color=#00FFFF>Escudos: " + _localHealth.CurrentShields + "/" + _localHealth.MaxShields + "</color>";
        string kills = "Kills: " + _localSession.Kills + "/" + MatchController.KillsToWin;
        playerStatsText.text = health + "\n" + shields + "\n" + kills;
    }

    private string getHealthColor(int health)
    {
        if (health <= 20)
        {
            return "#FF0000";
        }
        if (health <= 60)
        {
            return "#FFFF00";
        }
        return "#00FF00";
    }

    private static void EnsureDefaultFont()
    {
        if (s_defaultFont != null) return;
        s_defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
    }

    private TextMeshProUGUI NewTMP(string name, RectTransform parent)
    {
        var go = NewUI(name, parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        if (s_defaultFont != null) tmp.font = s_defaultFont;
        tmp.color = Color.white;
        tmp.text = "";
        return tmp;
    }

    private void BuildIfMissing()
    {
        EnsureDefaultFont();

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        RectTransform root = transform as RectTransform;
        if (root == null)
        {
            gameObject.AddComponent<RectTransform>();
            root = (RectTransform)transform;
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
        }

        if (playerIdentityText == null)
        {
            var tmp = NewTMP("HUD_PlayerIdentity", root);
            var rt = (RectTransform)tmp.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(20f, -20f);
            rt.sizeDelta = new Vector2(400f, 32f);
            tmp.fontSize = 22;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.text = "Eres: Jugador ?";
            playerIdentityText = tmp;
        }

        if (playerStatsText == null)
        {
            var tmp = NewTMP("HUD_PlayerStats", root);
            var rt = (RectTransform)tmp.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(20f, -55f);
            rt.sizeDelta = new Vector2(400f, 100f);
            tmp.fontSize = 22;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            playerStatsText = tmp;
        }

        if (weapon1Icon == null || weapon2Icon == null)
        {
            var row = NewUI("HUD_WeaponRow", root);
            var rowRt = (RectTransform)row.transform;
            rowRt.anchorMin = new Vector2(0.5f, 0f);
            rowRt.anchorMax = new Vector2(0.5f, 0f);
            rowRt.pivot = new Vector2(0.5f, 0f);
            rowRt.anchoredPosition = new Vector2(0f, 20f);
            rowRt.sizeDelta = new Vector2(200f, 64f);

            var w1 = NewUI("Weapon1", (RectTransform)row.transform);
            var w1Rt = (RectTransform)w1.transform;
            w1Rt.anchorMin = new Vector2(0f, 0f);
            w1Rt.anchorMax = new Vector2(0.5f, 1f);
            w1Rt.offsetMin = new Vector2(0f, 0f);
            w1Rt.offsetMax = new Vector2(0f, 0f);
            weapon1Icon = w1.AddComponent<Image>();
            weapon1Icon.color = weaponActiveColor;

            var w2 = NewUI("Weapon2", (RectTransform)row.transform);
            var w2Rt = (RectTransform)w2.transform;
            w2Rt.anchorMin = new Vector2(0.5f, 0f);
            w2Rt.anchorMax = new Vector2(1f, 1f);
            w2Rt.offsetMin = new Vector2(0f, 0f);
            w2Rt.offsetMax = new Vector2(0f, 0f);
            weapon2Icon = w2.AddComponent<Image>();
            weapon2Icon.color = weaponInactiveColor;
        }

        if (killFeedText == null)
        {
            var tmp = NewTMP("HUD_KillFeed", root);
            var rt = (RectTransform)tmp.transform;
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -100f);
            rt.sizeDelta = new Vector2(700f, 220f);
            tmp.fontSize = 20;
            tmp.alignment = TextAlignmentOptions.Top;
            tmp.text = "";
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Overflow;
            killFeedText = tmp;
        }

        if (victoryPanel == null)
        {
            victoryPanel = NewPanel("HUD_VictoryPanel", root, new Color(0f, 0.6f, 0f, 0.7f), "VICTORIA");
            victoryPanel.SetActive(false);
        }
        if (defeatPanel == null)
        {
            defeatPanel = NewPanel("HUD_DefeatPanel", root, new Color(0.6f, 0f, 0f, 0.7f), "DERROTA");
            defeatPanel.SetActive(false);
        }
        if (backToMenuButton == null && victoryPanel != null)
        {
            var go = NewUI("BackToMenuBtn", (RectTransform)victoryPanel.transform);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 20f);
            rt.sizeDelta = new Vector2(220f, 56f);
            go.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);

            var textGo = NewUI("Text", (RectTransform)go.transform);
            var textRt = (RectTransform)textGo.transform;
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
            var tt = textGo.AddComponent<TextMeshProUGUI>();
            if (s_defaultFont != null) tt.font = s_defaultFont;
            tt.fontSize = 22;
            tt.alignment = TextAlignmentOptions.Center;
            tt.color = Color.white;
            tt.text = "Volver al menú";

            backToMenuButton = go.AddComponent<Button>();
            backToMenuButton.onClick.AddListener(() => {
                var back = FindFirstObjectByType<BackToMenuButton>();
                if (back != null) back.OnClickBackToMenu();
            });
            Button sameOnDefeat = backToMenuButton;
        }
    }

    private static GameObject NewUI(string name, RectTransform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private GameObject NewPanel(string name, RectTransform parent, Color color, string label)
    {
        var go = NewUI(name, parent);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(600f, 320f);
        go.AddComponent<Image>().color = color;

        var titleGo = NewUI("Title", rt);
        var titleRt = (RectTransform)titleGo.transform;
        titleRt.anchorMin = Vector2.zero;
        titleRt.anchorMax = Vector2.one;
        titleRt.offsetMin = Vector2.zero;
        titleRt.offsetMax = Vector2.zero;
        var t = titleGo.AddComponent<TextMeshProUGUI>();
        if (s_defaultFont != null) t.font = s_defaultFont;
        t.fontSize = 64;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;
        t.text = label;

        return go;
    }
}

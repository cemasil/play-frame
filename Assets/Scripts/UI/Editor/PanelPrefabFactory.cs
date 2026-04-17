using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using PlayFrame.UI.Base;
using PlayFrame.UI.Panels;

namespace PlayFrame.UI.Editor
{
    /// <summary>
    /// Editor menu items to create ready-to-use panel prefabs.
    /// Each panel is created with a full UI layout, requiring only visual customization.
    /// Access via: Create → PlayFrame → Panel → [PanelType]
    /// </summary>
    public static class PanelPrefabFactory
    {
        private const string MenuRoot = "GameObject/PlayFrame/Panel/";

        // Colors
        private static readonly Color PanelBg = new Color(0.12f, 0.12f, 0.16f, 0.95f);
        private static readonly Color HeaderBg = new Color(0.08f, 0.08f, 0.12f, 1f);
        private static readonly Color ButtonPrimary = new Color(0.2f, 0.6f, 0.9f, 1f);
        private static readonly Color ButtonDanger = new Color(0.85f, 0.25f, 0.25f, 1f);
        private static readonly Color ButtonSecondary = new Color(0.35f, 0.35f, 0.4f, 1f);
        private static readonly Color ButtonSuccess = new Color(0.2f, 0.75f, 0.3f, 1f);
        private static readonly Color TextWhite = Color.white;
        private static readonly Color TextGray = new Color(0.7f, 0.7f, 0.7f, 1f);

        #region Menu Items

        [MenuItem(MenuRoot + "Settings Panel", false, 10)]
        public static void CreateSettingsPanel(MenuCommand cmd)
        {
            var root = CreatePanelRoot<SettingsPanel>("SettingsPanel", cmd);
            var content = GetContent(root);

            // Title
            CreateHeader(content, "Settings");

            // Audio section
            var audioSection = CreateSection(content, "Audio");
            CreateLabeledSlider(audioSection, "Music Volume", "musicVolumeSlider");
            CreateLabeledSlider(audioSection, "SFX Volume", "sfxVolumeSlider");
            CreateLabeledToggle(audioSection, "Music", "musicMuteToggle");
            CreateLabeledToggle(audioSection, "SFX", "sfxMuteToggle");

            // Haptic
            CreateLabeledToggle(content, "Haptic Feedback", "hapticToggle");

            // Language
            CreateDropdown(content, "Language", "languageDropdown");

            // Buttons
            var buttons = CreateHorizontalGroup(content, "Buttons", 10f);
            CreateButton(buttons, "Reset", "resetButton", ButtonSecondary, 140, 50);
            CreateButton(buttons, "Restore Purchases", "restorePurchasesButton", ButtonPrimary, 200, 50);

            // Close button (top right)
            CreateCloseButton(root, "closeButton");

            // Delete save is hidden by default
            var del = CreateButton(content, "Delete Save Data", "deleteSaveButton", ButtonDanger, 200, 45);
            del.gameObject.SetActive(false);

            FinalizePrefab(root);
        }

        [MenuItem(MenuRoot + "Level Complete Panel", false, 20)]
        public static void CreateLevelCompletePanel(MenuCommand cmd)
        {
            var root = CreatePanelRoot<LevelCompletePanel>("LevelCompletePanel", cmd);
            var content = GetContent(root);

            // Main View
            var mainView = CreateSubView(content, "MainView");
            CreateHeader(mainView, "Level Complete!");
            CreateText(mainView, "Level 1", 28, "levelText");
            CreateText(mainView, "12,500", 48, "scoreText");

            // Stars
            var starRow = CreateHorizontalGroup(mainView, "Stars", 15f);
            for (int i = 0; i < 3; i++)
                CreateStarImage(starRow, $"Star_{i}");

            var mainButtons = CreateVerticalGroup(mainView, "Buttons", 10f);
            CreateButton(mainButtons, "Next Level", "nextLevelButton", ButtonPrimary, 250, 55);
            CreateButton(mainButtons, "Replay", "replayButton", ButtonSecondary, 250, 55);
            CreateButton(mainButtons, "Home", "homeButton", ButtonSecondary, 250, 55);
            CreateButton(mainButtons, "Double Rewards (Ad)", "doubleRewardsButton", ButtonSuccess, 250, 55);

            // Rewards View
            var rewardsView = CreateSubView(content, "RewardsView");
            CreateHeader(rewardsView, "Rewards");
            CreateText(rewardsView, "+100", 42, "coinsEarnedText");
            CreateText(rewardsView, "+50 Bonus", 24, "bonusText");
            CreateButton(rewardsView, "Claim", "claimRewardsButton", ButtonPrimary, 200, 55);
            rewardsView.SetActive(false);

            // Booster View
            var boosterView = CreateSubView(content, "BoosterView");
            CreateHeader(boosterView, "Booster Earned!");
            CreateIcon(boosterView, 100, "boosterIcon");
            CreateText(boosterView, "Color Bomb", 28, "boosterNameText");
            CreateText(boosterView, "Destroys all pieces of one color", 18, "boosterDescText");
            CreateButton(boosterView, "Claim", "claimBoosterButton", ButtonPrimary, 200, 55);
            boosterView.SetActive(false);

            FinalizePrefab(root);
        }

        [MenuItem(MenuRoot + "Level Failed Panel", false, 30)]
        public static void CreateLevelFailedPanel(MenuCommand cmd)
        {
            var root = CreatePanelRoot<LevelFailedPanel>("LevelFailedPanel", cmd);
            var content = GetContent(root);

            // Main View
            var mainView = CreateSubView(content, "MainView");
            CreateHeader(mainView, "Level Failed");
            CreateText(mainView, "Level 1", 28, "levelText");
            CreateText(mainView, "Out of moves!", 22, "messageText");

            var mainBtns = CreateVerticalGroup(mainView, "Buttons", 10f);
            CreateButton(mainBtns, "Retry", "retryButton", ButtonPrimary, 250, 55);
            CreateButton(mainBtns, "Extra Moves", "extraMovesButton", ButtonSuccess, 250, 55);
            CreateButton(mainBtns, "Watch Ad", "watchAdButton", ButtonSuccess, 250, 55);
            CreateButton(mainBtns, "Home", "homeButton", ButtonSecondary, 250, 55);
            CreateButton(mainBtns, "Give Up", "giveUpButton", ButtonDanger, 250, 45);

            // Extra Moves View
            var extraView = CreateSubView(content, "ExtraMovesView");
            CreateHeader(extraView, "Extra Moves");
            CreateText(extraView, "+5 Moves", 36, "extraMovesCountText");
            CreateText(extraView, "100", 28, "extraMovesCostText");
            var extraBtns = CreateVerticalGroup(extraView, "Buttons", 10f);
            CreateButton(extraBtns, "Buy with Coins", "buyExtraMovesCoinsButton", ButtonPrimary, 250, 55);
            CreateButton(extraBtns, "Buy with IAP", "buyExtraMovesIAPButton", ButtonSuccess, 250, 55);
            CreateButton(extraBtns, "Cancel", "cancelSubViewButton", ButtonSecondary, 200, 45);
            extraView.SetActive(false);

            // Watch Ad View
            var adView = CreateSubView(content, "WatchAdView");
            CreateHeader(adView, "Watch Ad");
            CreateText(adView, "Watch a short video for +5 moves", 22, "adRewardText");
            CreateButton(adView, "Watch Now", "confirmWatchAdButton", ButtonPrimary, 250, 55);
            adView.SetActive(false);

            // Give Up View
            var giveUpView = CreateSubView(content, "GiveUpView");
            CreateHeader(giveUpView, "Give Up?");
            CreateText(giveUpView, "You will lose all progress on this level.", 20, "giveUpMessageText");
            CreateButton(giveUpView, "Yes, Give Up", "confirmGiveUpButton", ButtonDanger, 250, 55);
            giveUpView.SetActive(false);

            FinalizePrefab(root);
        }

        [MenuItem(MenuRoot + "IAP Panel", false, 40)]
        public static void CreateIAPPanel(MenuCommand cmd)
        {
            var root = CreatePanelRoot<IAPPanel>("IAPPanel", cmd);
            var content = GetContent(root);

            // Store View
            var storeView = CreateSubView(content, "StoreView");
            CreateHeader(storeView, "Store");

            var container = CreateVerticalGroup(storeView, "ProductContainer", 10f);
            // Placeholder products
            for (int i = 1; i <= 3; i++)
                CreateText(container.transform, $"Product {i} — $0.99", 20);

            var storeBtns = CreateHorizontalGroup(storeView, "Buttons", 10f);
            CreateButton(storeBtns, "Restore", "restoreButton", ButtonSecondary, 150, 50);
            CreateCloseButton(root, "closeButton");

            // Purchase Success View
            var successView = CreateSubView(content, "PurchaseSuccessView");
            CreateIcon(successView, 80, "purchaseSuccessIcon");
            CreateText(successView, "Purchase Successful!", 26, "purchaseSuccessText");
            CreateButton(successView, "OK", "okSuccessButton", ButtonPrimary, 200, 50);
            successView.SetActive(false);

            // Purchase Error View
            var errorView = CreateSubView(content, "PurchaseErrorView");
            CreateText(errorView, "Purchase Failed", 26, "purchaseErrorText");
            var errBtns = CreateHorizontalGroup(errorView, "Buttons", 10f);
            CreateButton(errBtns, "OK", "okErrorButton", ButtonSecondary, 120, 50);
            CreateButton(errBtns, "Retry", "retryButton", ButtonPrimary, 120, 50);
            errorView.SetActive(false);

            // Restore Success View
            var restSuccView = CreateSubView(content, "RestoreSuccessView");
            CreateText(restSuccView, "3 purchases restored!", 26, "restoreSuccessText");
            CreateButton(restSuccView, "OK", "okRestoreSuccessButton", ButtonPrimary, 200, 50);
            restSuccView.SetActive(false);

            // Restore Error View
            var restErrView = CreateSubView(content, "RestoreErrorView");
            CreateText(restErrView, "Restore failed.", 26, "restoreErrorText");
            CreateButton(restErrView, "OK", "okRestoreErrorButton", ButtonSecondary, 200, 50);
            restErrView.SetActive(false);

            FinalizePrefab(root);
        }

        [MenuItem(MenuRoot + "Tutorial Panel", false, 50)]
        public static void CreateTutorialPanel(MenuCommand cmd)
        {
            var root = CreatePanelRoot<TutorialPanel>("TutorialPanel", cmd);
            root.GetComponent<TutorialPanel>().enabled = true;
            var content = GetContent(root);

            CreateText(content, "Welcome!", 32, "titleText");
            CreateIcon(content, 150, "tutorialImage");
            CreateText(content, "Tap pieces to select them.", 20, "descriptionText");
            CreateText(content, "1 / 3", 16, "stepCounterText");

            var btns = CreateHorizontalGroup(content, "Buttons", 10f);
            CreateButton(btns, "Back", "prevButton", ButtonSecondary, 120, 50);
            CreateButton(btns, "Next", "nextButton", ButtonPrimary, 120, 50);
            CreateButton(btns, "Done", "doneButton", ButtonSuccess, 120, 50);
            CreateButton(content, "Skip", "skipButton", ButtonSecondary, 100, 40);

            FinalizePrefab(root);
        }

        [MenuItem(MenuRoot + "Special Offer Panel", false, 60)]
        public static void CreateSpecialOfferPanel(MenuCommand cmd)
        {
            var root = CreatePanelRoot<SpecialOfferPanel>("SpecialOfferPanel", cmd);
            var content = GetContent(root);

            CreateText(content, "Weekend Deal!", 32, "titleText");
            CreateIcon(content, 120, "offerImage");
            CreateText(content, "50 Gems + 1000 Coins", 22, "descriptionText");

            var priceRow = CreateHorizontalGroup(content, "PriceRow", 10f);
            CreateText(priceRow.transform, "$4.99", 18, "originalPriceText");
            CreateText(priceRow.transform, "$2.99", 28, "priceText");
            CreateText(content, "-40%", 20, "discountBadgeText");

            // Timer
            var timerContainer = CreateSubView(content, "TimerContainer");
            CreateText(timerContainer, "23:59:59", 24, "timerText");

            CreateButton(content, "$2.99", "purchaseButton", ButtonPrimary, 250, 60);
            CreateCloseButton(root, "closeButton");

            FinalizePrefab(root);
        }

        [MenuItem(MenuRoot + "Info Panel", false, 70)]
        public static void CreateInfoPanel(MenuCommand cmd)
        {
            var root = CreatePanelRoot<InfoPanel>("InfoPanel", cmd);
            var content = GetContent(root);

            CreateText(content, "Information", 28, "titleText");
            CreateIcon(content, 80, "iconImage");
            CreateText(content, "This is an info message.", 20, "messageText");

            var btns = CreateHorizontalGroup(content, "Buttons", 15f);
            CreateButton(btns, "OK", "primaryButton", ButtonPrimary, 150, 50);
            var sec = CreateButton(btns, "Cancel", "secondaryButton", ButtonSecondary, 150, 50);
            sec.gameObject.SetActive(false);

            CreateCloseButton(root, "closeButton");

            FinalizePrefab(root);
        }

        [MenuItem(MenuRoot + "Pause Panel", false, 80)]
        public static void CreatePausePanel(MenuCommand cmd)
        {
            var root = CreatePanelRoot<PausePanel>("PausePanel", cmd);
            var content = GetContent(root);

            CreateText(content, "Paused", 36, "titleText");
            CreateText(content, "Level 5", 22, "levelText");

            // Quick audio toggles
            var toggleRow = CreateHorizontalGroup(content, "AudioToggles", 20f);
            CreateLabeledToggle(toggleRow, "Music", "musicToggle");
            CreateLabeledToggle(toggleRow, "SFX", "sfxToggle");

            var btns = CreateVerticalGroup(content, "Buttons", 10f);
            CreateButton(btns, "Resume", "resumeButton", ButtonPrimary, 250, 55);
            CreateButton(btns, "Restart", "restartButton", ButtonSecondary, 250, 55);
            CreateButton(btns, "Settings", "settingsButton", ButtonSecondary, 250, 55);
            CreateButton(btns, "Home", "homeButton", ButtonSecondary, 250, 55);

            FinalizePrefab(root);
        }

        [MenuItem(MenuRoot + "Confirmation Panel", false, 90)]
        public static void CreateConfirmationPanel(MenuCommand cmd)
        {
            var root = CreatePanelRoot<ConfirmationPanel>("ConfirmationPanel", cmd);
            var content = GetContent(root);

            CreateText(content, "Are you sure?", 28, "titleText");
            CreateIcon(content, 60, "iconImage");
            CreateText(content, "This action cannot be undone.", 20, "messageText");

            var btns = CreateHorizontalGroup(content, "Buttons", 15f);
            CreateButton(btns, "Yes", "confirmButton", ButtonDanger, 150, 50);
            CreateButton(btns, "No", "cancelButton", ButtonSecondary, 150, 50);

            FinalizePrefab(root);
        }

        #endregion

        #region Factory Helpers

        private static GameObject CreatePanelRoot<T>(string name, MenuCommand cmd) where T : UIPanel
        {
            var parent = cmd?.context as GameObject;

            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(T));
            GameObjectUtility.SetParentAndAlign(go, parent);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = go.GetComponent<Image>();
            img.color = PanelBg;
            img.raycastTarget = true;

            // Content container with VerticalLayoutGroup
            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(go.transform, false);

            var crt = contentGo.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.05f, 0.05f);
            crt.anchorMax = new Vector2(0.95f, 0.95f);
            crt.offsetMin = Vector2.zero;
            crt.offsetMax = Vector2.zero;

            var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.spacing = 15f;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            Selection.activeGameObject = go;

            return go;
        }

        private static Transform GetContent(GameObject root)
        {
            return root.transform.Find("Content");
        }

        private static void CreateHeader(Transform parent, string text)
        {
            var go = new GameObject("Header", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 60);

            var img = go.GetComponent<Image>();
            img.color = HeaderBg;

            var txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtGo.transform.SetParent(go.transform, false);
            var trt = txtGo.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            var tmp = txtGo.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 30;
            tmp.color = TextWhite;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
        }

        private static void CreateHeader(GameObject parent, string text)
        {
            CreateHeader(parent.transform, text);
        }

        private static TextMeshProUGUI CreateText(Transform parent, string text, int fontSize, string objectName = null)
        {
            var go = new GameObject(objectName ?? "Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, fontSize + 20);

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = TextWhite;
            tmp.alignment = TextAlignmentOptions.Center;

            return tmp;
        }

        private static TextMeshProUGUI CreateText(GameObject parent, string text, int fontSize, string objectName = null)
        {
            return CreateText(parent.transform, text, fontSize, objectName);
        }

        private static Button CreateButton(Transform parent, string label, string objectName, Color bgColor, float width, float height)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(width, height);

            var le = go.GetComponent<LayoutElement>();
            le.preferredWidth = width;
            le.preferredHeight = height;
            le.minHeight = height;

            var img = go.GetComponent<Image>();
            img.color = bgColor;

            var txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtGo.transform.SetParent(go.transform, false);
            var trt = txtGo.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(5, 0);
            trt.offsetMax = new Vector2(-5, 0);

            var tmp = txtGo.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 20;
            tmp.color = TextWhite;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;

            return go.GetComponent<Button>();
        }

        private static Button CreateButton(GameObject parent, string label, string objectName, Color bgColor, float width, float height)
        {
            return CreateButton(parent.transform, label, objectName, bgColor, width, height);
        }

        private static Button CreateButton(Component parent, string label, string objectName, Color bgColor, float width, float height)
        {
            return CreateButton(parent.transform, label, objectName, bgColor, width, height);
        }

        private static void CreateCloseButton(GameObject root, string objectName)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(root.transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-10, -10);
            rt.sizeDelta = new Vector2(40, 40);

            var img = go.GetComponent<Image>();
            img.color = ButtonSecondary;

            var txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtGo.transform.SetParent(go.transform, false);
            var trt = txtGo.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            var tmp = txtGo.GetComponent<TextMeshProUGUI>();
            tmp.text = "X";
            tmp.fontSize = 22;
            tmp.color = TextWhite;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
        }

        private static void CreateLabeledSlider(Transform parent, string label, string objectName)
        {
            var row = new GameObject(objectName, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);

            var le = row.GetComponent<LayoutElement>();
            le.preferredHeight = 45;
            le.minHeight = 45;

            var hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.padding = new RectOffset(5, 5, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Label
            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            labelGo.transform.SetParent(row.transform, false);
            var labelLe = labelGo.GetComponent<LayoutElement>();
            labelLe.preferredWidth = 130;
            labelLe.minWidth = 100;
            var tmp = labelGo.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 18;
            tmp.color = TextGray;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Slider
            CreateSliderControl(row.transform, objectName + "_Slider");
        }

        private static void CreateLabeledSlider(GameObject parent, string label, string objectName)
        {
            CreateLabeledSlider(parent.transform, label, objectName);
        }

        private static void CreateSliderControl(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Slider), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var sliderLe = go.GetComponent<LayoutElement>();
            sliderLe.preferredWidth = 200;
            sliderLe.minWidth = 120;
            sliderLe.flexibleWidth = 1;

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200, 30);

            var slider = go.GetComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 0.8f;

            // Background
            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(go.transform, false);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0, 0.3f);
            bgRt.anchorMax = new Vector2(1, 0.7f);
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f);

            // Fill area
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            var faRt = fillArea.GetComponent<RectTransform>();
            faRt.anchorMin = new Vector2(0, 0.3f);
            faRt.anchorMax = new Vector2(1, 0.7f);
            faRt.offsetMin = new Vector2(5, 0);
            faRt.offsetMax = new Vector2(-5, 0);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            var fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = Vector2.zero;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            fill.GetComponent<Image>().color = ButtonPrimary;

            // Handle slide area
            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(go.transform, false);
            var haRt = handleArea.GetComponent<RectTransform>();
            haRt.anchorMin = Vector2.zero;
            haRt.anchorMax = Vector2.one;
            haRt.offsetMin = new Vector2(10, 0);
            haRt.offsetMax = new Vector2(-10, 0);

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            var hRt = handle.GetComponent<RectTransform>();
            hRt.sizeDelta = new Vector2(24, 24);
            handle.GetComponent<Image>().color = TextWhite;

            slider.targetGraphic = handle.GetComponent<Image>();
            slider.fillRect = fillRt;
            slider.handleRect = hRt;
        }

        private static void CreateLabeledToggle(Transform parent, string label, string objectName)
        {
            var row = new GameObject(objectName, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);

            var le = row.GetComponent<LayoutElement>();
            le.preferredHeight = 45;
            le.minHeight = 45;

            var hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.padding = new RectOffset(5, 5, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Label
            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            labelGo.transform.SetParent(row.transform, false);
            var labelLe = labelGo.GetComponent<LayoutElement>();
            labelLe.preferredWidth = 130;
            labelLe.minWidth = 100;
            labelLe.preferredHeight = 35;
            var tmp = labelGo.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 18;
            tmp.color = TextGray;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Toggle
            var toggleGo = new GameObject("Toggle", typeof(RectTransform), typeof(Toggle), typeof(LayoutElement));
            toggleGo.transform.SetParent(row.transform, false);
            var toggleLe = toggleGo.GetComponent<LayoutElement>();
            toggleLe.preferredWidth = 50;
            toggleLe.preferredHeight = 30;
            toggleLe.minWidth = 50;
            toggleLe.minHeight = 30;

            var toggleRt = toggleGo.GetComponent<RectTransform>();
            toggleRt.sizeDelta = new Vector2(50, 30);

            var bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(toggleGo.transform, false);
            var bgrt = bgGo.GetComponent<RectTransform>();
            bgrt.anchorMin = Vector2.zero;
            bgrt.anchorMax = Vector2.one;
            bgrt.offsetMin = Vector2.zero;
            bgrt.offsetMax = Vector2.zero;
            bgGo.GetComponent<Image>().color = ButtonSecondary;

            var checkGo = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            checkGo.transform.SetParent(bgGo.transform, false);
            var ckRt = checkGo.GetComponent<RectTransform>();
            ckRt.anchorMin = new Vector2(0.1f, 0.1f);
            ckRt.anchorMax = new Vector2(0.9f, 0.9f);
            ckRt.offsetMin = Vector2.zero;
            ckRt.offsetMax = Vector2.zero;
            checkGo.GetComponent<Image>().color = ButtonPrimary;

            var toggle = toggleGo.GetComponent<Toggle>();
            toggle.isOn = true;
            toggle.targetGraphic = bgGo.GetComponent<Image>();
            toggle.graphic = checkGo.GetComponent<Image>();
        }

        private static void CreateLabeledToggle(GameObject parent, string label, string objectName)
        {
            CreateLabeledToggle(parent.transform, label, objectName);
        }

        private static void CreateLabeledToggle(Component parent, string label, string objectName)
        {
            CreateLabeledToggle(parent.transform, label, objectName);
        }

        private static void CreateDropdown(Transform parent, string label, string objectName)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(TMP_Dropdown), typeof(Image), typeof(LayoutElement));
            go.transform.SetParent(parent, false);

            var le = go.GetComponent<LayoutElement>();
            le.preferredHeight = 45;
            le.minHeight = 45;

            go.GetComponent<Image>().color = ButtonSecondary;

            // Caption text
            var captionGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            captionGo.transform.SetParent(go.transform, false);
            var crt = captionGo.GetComponent<RectTransform>();
            crt.anchorMin = Vector2.zero;
            crt.anchorMax = Vector2.one;
            crt.offsetMin = new Vector2(10, 0);
            crt.offsetMax = new Vector2(-30, 0);
            var captionTmp = captionGo.GetComponent<TextMeshProUGUI>();
            captionTmp.text = label;
            captionTmp.fontSize = 18;
            captionTmp.color = TextWhite;
            captionTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Arrow
            var arrowGo = new GameObject("Arrow", typeof(RectTransform), typeof(Image));
            arrowGo.transform.SetParent(go.transform, false);
            var arrowRt = arrowGo.GetComponent<RectTransform>();
            arrowRt.anchorMin = new Vector2(1, 0);
            arrowRt.anchorMax = new Vector2(1, 1);
            arrowRt.pivot = new Vector2(1, 0.5f);
            arrowRt.sizeDelta = new Vector2(25, 25);
            arrowRt.anchoredPosition = new Vector2(-8, 0);
            arrowGo.GetComponent<Image>().color = TextGray;

            // === Template (required by TMP_Dropdown) ===
            var template = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            template.transform.SetParent(go.transform, false);
            var trt = template.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0, 0);
            trt.anchorMax = new Vector2(1, 0);
            trt.pivot = new Vector2(0.5f, 1f);
            trt.anchoredPosition = Vector2.zero;
            trt.sizeDelta = new Vector2(0, 150);
            template.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 1f);

            // Viewport
            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
            viewport.transform.SetParent(template.transform, false);
            var vpRt = viewport.GetComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = Vector2.zero;
            vpRt.offsetMax = Vector2.zero;
            viewport.GetComponent<Image>().color = new Color(1, 1, 1, 0.003f); // near-transparent for Mask
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            // Content
            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(viewport.transform, false);
            var contentRt = contentGo.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = new Vector2(0, 28);

            // Item (template row with Toggle)
            var item = new GameObject("Item", typeof(RectTransform), typeof(Toggle));
            item.transform.SetParent(contentGo.transform, false);
            var itemRt = item.GetComponent<RectTransform>();
            itemRt.anchorMin = new Vector2(0, 0.5f);
            itemRt.anchorMax = new Vector2(1, 0.5f);
            itemRt.sizeDelta = new Vector2(0, 28);

            // Item background
            var itemBg = new GameObject("Item Background", typeof(RectTransform), typeof(Image));
            itemBg.transform.SetParent(item.transform, false);
            var ibRt = itemBg.GetComponent<RectTransform>();
            ibRt.anchorMin = Vector2.zero;
            ibRt.anchorMax = Vector2.one;
            ibRt.offsetMin = Vector2.zero;
            ibRt.offsetMax = Vector2.zero;
            itemBg.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.3f, 1f);

            // Item checkmark
            var itemCheck = new GameObject("Item Checkmark", typeof(RectTransform), typeof(Image));
            itemCheck.transform.SetParent(item.transform, false);
            var icRt = itemCheck.GetComponent<RectTransform>();
            icRt.anchorMin = new Vector2(0, 0.5f);
            icRt.anchorMax = new Vector2(0, 0.5f);
            icRt.sizeDelta = new Vector2(20, 20);
            icRt.anchoredPosition = new Vector2(15, 0);
            itemCheck.GetComponent<Image>().color = ButtonPrimary;

            // Item label
            var itemLabel = new GameObject("Item Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            itemLabel.transform.SetParent(item.transform, false);
            var ilRt = itemLabel.GetComponent<RectTransform>();
            ilRt.anchorMin = Vector2.zero;
            ilRt.anchorMax = Vector2.one;
            ilRt.offsetMin = new Vector2(30, 0);
            ilRt.offsetMax = new Vector2(-10, 0);
            var itemTmp = itemLabel.GetComponent<TextMeshProUGUI>();
            itemTmp.fontSize = 16;
            itemTmp.color = TextWhite;
            itemTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Wire Toggle
            var itemToggle = item.GetComponent<Toggle>();
            itemToggle.targetGraphic = itemBg.GetComponent<Image>();
            itemToggle.graphic = itemCheck.GetComponent<Image>();
            itemToggle.isOn = true;

            // Wire ScrollRect
            var scrollRect = template.GetComponent<ScrollRect>();
            scrollRect.content = contentRt;
            scrollRect.viewport = vpRt;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            // Wire TMP_Dropdown
            var dd = go.GetComponent<TMP_Dropdown>();
            dd.captionText = captionTmp;
            dd.itemText = itemTmp;
            dd.template = trt;

            // Deactivate template (TMP_Dropdown expects it inactive)
            template.SetActive(false);
        }

        private static void CreateDropdown(GameObject parent, string label, string objectName)
        {
            CreateDropdown(parent.transform, label, objectName);
        }

        private static HorizontalLayoutGroup CreateHorizontalGroup(Transform parent, string name, float spacing)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            go.transform.SetParent(parent, false);

            var le = go.GetComponent<LayoutElement>();
            le.preferredHeight = 60;

            var hlg = go.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = spacing;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            return hlg;
        }

        private static HorizontalLayoutGroup CreateHorizontalGroup(GameObject parent, string name, float spacing)
        {
            return CreateHorizontalGroup(parent.transform, name, spacing);
        }

        private static VerticalLayoutGroup CreateVerticalGroup(Transform parent, string name, float spacing)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup));
            go.transform.SetParent(parent, false);

            var vlg = go.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = spacing;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = false;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            return vlg;
        }

        private static VerticalLayoutGroup CreateVerticalGroup(GameObject parent, string name, float spacing)
        {
            return CreateVerticalGroup(parent.transform, name, spacing);
        }

        private static GameObject CreateSubView(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var vlg = go.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 12f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.padding = new RectOffset(5, 5, 5, 5);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            return go;
        }

        private static GameObject CreateSubView(GameObject parent, string name)
        {
            return CreateSubView(parent.transform, name);
        }

        private static GameObject CreateSection(Transform parent, string title)
        {
            var go = new GameObject(title + "Section", typeof(RectTransform), typeof(VerticalLayoutGroup));
            go.transform.SetParent(parent, false);

            var vlg = go.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Section title
            var labelGo = new GameObject("SectionTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            labelGo.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 30);
            var tmp = labelGo.GetComponent<TextMeshProUGUI>();
            tmp.text = title;
            tmp.fontSize = 20;
            tmp.color = TextGray;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;

            return go;
        }

        private static GameObject CreateSection(GameObject parent, string title)
        {
            return CreateSection(parent.transform, title);
        }

        private static Image CreateIcon(Transform parent, float size, string objectName)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            go.transform.SetParent(parent, false);

            var le = go.GetComponent<LayoutElement>();
            le.preferredWidth = size;
            le.preferredHeight = size;
            le.minHeight = size;

            var img = go.GetComponent<Image>();
            img.color = TextGray;
            img.preserveAspect = true;

            return img;
        }

        private static Image CreateIcon(GameObject parent, float size, string objectName)
        {
            return CreateIcon(parent.transform, size, objectName);
        }

        private static void CreateStarImage(HorizontalLayoutGroup parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            go.transform.SetParent(parent.transform, false);

            var le = go.GetComponent<LayoutElement>();
            le.preferredWidth = 50;
            le.preferredHeight = 50;

            go.GetComponent<Image>().color = new Color(1f, 0.85f, 0f);
        }

        private static void FinalizePrefab(GameObject root)
        {
            // Auto-wire serialized fields to matching child objects
            AutoWirePanel(root);

            EditorUtility.SetDirty(root);
        }

        /// <summary>
        /// Automatically assigns serialized fields on the panel component
        /// by matching field names to child GameObject names in the hierarchy.
        /// </summary>
        private static void AutoWirePanel(GameObject root)
        {
            var panel = root.GetComponent<UIPanel>();
            if (panel == null) return;

            var so = new SerializedObject(panel);
            so.Update();

            var iter = so.GetIterator();
            while (iter.NextVisible(true))
            {
                if (iter.propertyType == SerializedPropertyType.ObjectReference && iter.objectReferenceValue == null)
                {
                    string fieldName = iter.name;
                    var child = FindChildRecursive(root.transform, fieldName);
                    if (child == null) continue;

                    // Try to match the type
                    var fieldType = iter.type;
                    if (fieldType.Contains("Button"))
                        iter.objectReferenceValue = child.GetComponent<Button>();
                    else if (fieldType.Contains("Slider"))
                        iter.objectReferenceValue = child.GetComponentInChildren<Slider>();
                    else if (fieldType.Contains("Toggle"))
                        iter.objectReferenceValue = child.GetComponentInChildren<Toggle>();
                    else if (fieldType.Contains("TMP_Dropdown"))
                        iter.objectReferenceValue = child.GetComponent<TMP_Dropdown>();
                    else if (fieldType.Contains("TextMeshProUGUI"))
                        iter.objectReferenceValue = child.GetComponent<TextMeshProUGUI>();
                    else if (fieldType.Contains("Image"))
                        iter.objectReferenceValue = child.GetComponent<Image>();
                    else if (fieldType.Contains("GameObject"))
                        iter.objectReferenceValue = child.gameObject;
                }
            }

            // Wire CanvasGroup
            var cgProp = so.FindProperty("canvasGroup");
            if (cgProp != null && cgProp.objectReferenceValue == null)
                cgProp.objectReferenceValue = root.GetComponent<CanvasGroup>();

            so.ApplyModifiedProperties();
        }

        private static Transform FindChildRecursive(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                var found = FindChildRecursive(child, name);
                if (found != null) return found;
            }
            return null;
        }

        #endregion
    }
}

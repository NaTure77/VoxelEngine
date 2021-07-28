using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
public class OptionUI2 : MonoBehaviour
{
    public GameObject optionPanel;

    public Toggle hudToggle;
    public Toggle gridToggle;
    public Toggle lightToggle;

    public Button importButton;
    // Start is called before the first frame update

    // Update is called once per frame

    private void Start()
    {
        ActivateUI(true);
        Controller3.instance.inputManager.Player.Esc.performed += val =>
        {
            ToggleUI();
        };
        Controller3.instance.inputManager.Player.Grid.performed += val =>
        {
            gridToggle.isOn = !gridToggle.isOn;
        };
        Controller3.instance.inputManager.Player.LightShadow.performed += val =>
        {
            lightToggle.isOn = !lightToggle.isOn;
        };

#if UNITY_IOS
        //Controller3.instance.SetGamepadMode(true);
       // importButton.gameObject.SetActive(true);
        if (importButton == null) return;
        importButton.onClick.AddListener(() =>
            {
                string[] extensionList = {"public.plain-text", "public.item"};
                NativeFilePicker.PickFile((path) =>
                {
                    if(File.Exists(path))
                    {
                        byte[] data = File.ReadAllBytes(path);
                        if (data != null)
                        {
                            File.WriteAllBytes(Application.persistentDataPath + "/" + Path.GetFileName(path), data);
                        }
                    }
            }, extensionList);

        });
#else
        hudToggle.isOn = false;
        if (importButton == null) return;
        importButton.onClick.AddListener(() =>
        {
            string[] extensionList = { "public.plain-text", "public.item" };

            SimpleFileBrowser.FileBrowser.OnSuccess success = (string[] s) =>
            {
                if (File.Exists(s[0]))
                {
                    byte[] data = File.ReadAllBytes(s[0]);
                    if (data != null)
                    {
                        File.WriteAllBytes(Application.persistentDataPath + "/" + Path.GetFileName(s[0]), data);
                    }
                }
            };
            SimpleFileBrowser.FileBrowser.OnCancel cancel = () => { };
            SimpleFileBrowser.FileBrowser.SetFilters(true, ".va", ".png", ".jpg", ".obj");
            //SimpleFileBrowser.FileBrowser.SetDefaultFilter(".txt");

            SimpleFileBrowser.FileBrowser.ShowLoadDialog
                (success, cancel, initialPath: Application.persistentDataPath, title: "Import file", loadButtonText: "Select");
        });
#endif
    }

    public void ToggleUI()
    {
        ActivateUI(!optionPanel.activeSelf);
    }

    void ActivateUI(bool activate)
    {
        if (activate) Controller3.instance.StopLoop();
        else Controller3.instance.StartLoop();
        optionPanel.SetActive(activate);
        Cursor.visible = activate;
        Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
    }
    public void ExitApplication()
    {
        Application.Quit();
    }
}

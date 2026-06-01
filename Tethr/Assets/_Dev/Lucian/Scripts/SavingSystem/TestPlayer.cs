using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayer : MonoBehaviour
{
    public int level = 1;
    public int coins = 0;

    public InputActionReference saveAction;
    public InputActionReference loadAction;
    public InputActionReference gainAction;
    public InputActionReference collectAction;

    public void GainExperience(int amount)
    {
        level += amount;
    }

    public void CollectCoins(int amount)
    {
        coins += amount;
    }

    private void OnEnable()
    {
        ToggleAction(saveAction, true, OnSave);
        ToggleAction(loadAction, true, OnLoad);
        ToggleAction(gainAction, true, OnGainExperience);
        ToggleAction(collectAction, true, OnCollectCoins);
    }

    private void OnDisable()
    {
        ToggleAction(saveAction, false, OnSave);
        ToggleAction(loadAction, false, OnLoad);
        ToggleAction(gainAction, false, OnGainExperience);
        ToggleAction(collectAction, false, OnCollectCoins);
    }

    private static void ToggleAction(InputActionReference actionReference, bool enable, System.Action<InputAction.CallbackContext> callback)
    {
        if (actionReference == null || actionReference.action == null)
        {
            return;
        }

        if (enable)
        {
            actionReference.action.performed += callback;
            actionReference.action.Enable();
        }
        else
        {
            actionReference.action.performed -= callback;
            actionReference.action.Disable();
        }
    }

    private void OnSave(InputAction.CallbackContext context)
    {
        SavingSystem.SavePlayer(this);
        Debug.Log("Game saved! Current level: " + level + ", coins: " + coins);
    }

    private void OnLoad(InputAction.CallbackContext context)
    {
        PlayerData data = SavingSystem.LoadPlayer();
        if (data != null)
        {
            level = data.level;
            coins = data.coins;
        }
        Debug.Log("Game loaded! Current level: " + level + ", coins: " + coins);
    }

    private void OnGainExperience(InputAction.CallbackContext context)
    {
        GainExperience(1);
        Debug.Log("Gained experience! Current level: " + level);
    }

    private void OnCollectCoins(InputAction.CallbackContext context)
    {
        CollectCoins(10);
        Debug.Log("Collected coins! Current coins: " + coins);
    }
}

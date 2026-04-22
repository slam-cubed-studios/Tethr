using TMPro;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [SerializeField] bool isLevelCompleted = false;
    [SerializeField] GameObject levelButton;
    [SerializeField] TMP_Text tooltip;
    [SerializeField] GameObject previousButton;

    TMP_Text levelButtonText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tooltip.text = "";
        previousButton.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Next()
    {
        //Only go next if the level is completed, otherwise do nothing
        if (isLevelCompleted)
        {
            //Get the text component of the level button and update it to the next level, then activate the previous button and reset the stars and the tooltip
            levelButtonText = levelButton.transform.GetChild(0).GetComponent<TMP_Text>();

            levelButtonText.text = "Level " + (int.Parse(levelButtonText.text.Substring(6)) + 1).ToString();

            for(int i = 1; i < levelButton.transform.childCount; i++)
            {
                //levelButton.GetComponentInChildren<Image>().color = Color.white;
                levelButton.transform.GetChild(i).GetComponent<Image>().color = Color.white;
            }

            tooltip.text = "";

            previousButton.SetActive(true);
        }
        else
        {
            tooltip.text = "Complete the level to proceed!";
            Debug.Log("Level not completed yet!");
        }
    }

    public void Previous()
    {
        //Get the text component of the level button and update it to the previous level, then deactivate the previous button and reset the stars and the tooltip
        levelButtonText = levelButton.transform.GetChild(0).GetComponent<TMP_Text>();
        levelButtonText.text = "Level " + (int.Parse(levelButtonText.text.Substring(6)) - 1).ToString();
        //levelButton.GetComponentInChildren<Image>().color = Color.white; Needs to be changed based on saved data of previous level
        tooltip.text = "";
        if (levelButtonText.text == "Level 1")
        {
            previousButton.SetActive(false);
        }
    }

    public void LoadLevel(string levelName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);
    }

    public void BackToMenu()
    {
        gameObject.SetActive(false);


    }
}

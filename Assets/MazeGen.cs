using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
//using TMPro;

public class MazeGen : MonoBehaviour
{
    public GameObject mazeCell;
    public GameObject mazeWall;
    GameObject currentCell;

    public GameObject widthSettings;
    public GameObject heightSettings;
    InputField widthInput;
    InputField heightInput;
    Slider widthSlider;
    Slider heightSlider;
    float width;
    float height;

    float posY;
    float posX;
    float distance;
    int mazeNum;

    Coroutine co;
    bool isRunning;

    bool saveMaze;
    int saveNum;
    int loaded;

    public InputField saveName;
    public List<Button> saveSlots;
    public List<Button> removeButtons;
    public Button saveButton;
    public GameObject errorText;

    float sec;
    public Button faster;
    public Button slower;
    public Button regenerate;
    public Button zoomIn;
    public Button zoomOut;

    public Camera mazeCam;
    bool isZoomed;
    float scale;

    List<GameObject> currentMazeCells;
    List<Transform> currentMazeWalls;

    List<GameObject> allMazes;
    List<float> widthHeith; 
    GameObject mazeEmpty;

    Stack<GameObject> visited;
    Stack<GameObject> finishedMaze;
    List<GameObject> neigbours;

    void Start()
    {
        allMazes = new List<GameObject>();
        widthHeith = new List<float>();

        widthInput = widthSettings.GetComponentInChildren<InputField>();
        heightInput = heightSettings.GetComponentInChildren<InputField>();

        widthSlider = widthSettings.GetComponentInChildren<Slider>();
        heightSlider = heightSettings.GetComponentInChildren<Slider>();

        widthSlider.value = 10;
        heightSlider.value = 10;

        saveMaze = false;
        loaded = -1;


        saveButton.interactable = false;
        saveButton.GetComponentInChildren<InputField>().interactable = false;
        for (int i = 0; i < saveSlots.Count; i++)
        {
            saveSlots[i].interactable = false;
            removeButtons[i].interactable = false;
        }

        faster.interactable = false;
        slower.interactable = false;
        regenerate.interactable = false;
        zoomIn.interactable = false;
        zoomOut.interactable = false;
        isZoomed = false;
    }

    private void Update()
    {
        widthInput.text = widthSlider.value.ToString();
        heightInput.text = heightSlider.value.ToString();

        if (isZoomed)
        {
            mazeCam.transform.position = new Vector3(currentCell.transform.position.x, currentCell.transform.position.y, -100);
        }
    }

    public void SaveCurrentMaze()
    {
        saveMaze = true;
        mazeEmpty.name = saveName.text;
        if (saveNum < saveSlots.Count)
        {
            int saveX;
            if (loaded >= 0)
            {
                saveX = loaded;
            }
            else
            {
                if (saveNum == 0)
                {
                    saveX = saveNum;
                }
                else
                {
                    int previous = saveNum - 1;
                    if (mazeEmpty == allMazes[previous])
                    {
                        saveX = previous;
                    }
                    else
                    {
                        saveX = saveNum;
                    }
                }
                if (saveX == saveNum)
                {
                    allMazes.Add(mazeEmpty);
                    saveSlots[saveX].interactable = true;
                    removeButtons[saveX].interactable = true;
                    saveNum++;
                }
            }
            saveSlots[saveX].GetComponentInChildren<Text>().text = saveName.text;
            if (width > height) 
            {
                widthHeith.Add(width);
            }
            else
            {
                widthHeith.Add(height);
            }
        }
        else
        {
            errorText.SetActive(true);
        }
    }

    public void LoadMaze(int x)
    {
        ZoomOut();
        if (isRunning)
        {
            StopCoroutine(co);
        }
        if (!saveMaze)
        {
            Object.Destroy(mazeEmpty);
        }
        loaded = x;
        mazeEmpty = allMazes[loaded];
        foreach(GameObject m in allMazes)
        {
            m.SetActive(false);
        }
        mazeEmpty.SetActive(true);
        width = widthHeith[loaded];

        zoomIn.interactable = false;

        scale = 20 / (width / 10);
        mazeEmpty.transform.localScale = new Vector3(scale, scale, 1);
    }

    public void RemoveSaved(int x)
    {
        GameObject remove = allMazes[x];
        if (mazeEmpty == remove)
        {
            if (isRunning)
            {
                StopCoroutine(co);
            }
            mazeEmpty = null;
            ZoomOut();
        }
        allMazes.Remove(remove);
        widthHeith.Remove(widthHeith[x]);
        Object.Destroy(remove);

        if(allMazes.Count == 0)
        {
            saveButton.interactable = false;
            saveButton.GetComponentInChildren<InputField>().interactable = false;
            regenerate.interactable = false;
            slower.interactable = false;
            faster.interactable = false;
            zoomIn.interactable = false;
            zoomOut.interactable = false;
        }

        for (int i = x; i < allMazes.Count; i++)
        {
            saveSlots[i].GetComponentInChildren<Text>().text = allMazes[i].name;

        }

        x = allMazes.Count;
        for (int i = x; i < saveSlots.Count; i++)
        {
            saveSlots[i].GetComponentInChildren<Text>().text = "";
            saveSlots[i].interactable = false;
            removeButtons[i].interactable = false;
        }
        if (errorText.activeSelf)
        {
            errorText.SetActive(false);
        }
        saveNum--; 
    }

    public void CreateMazeField()
    {
        if (isZoomed)
        {
            ZoomOut();
        }
        if (loaded >= 0)
        {
            allMazes[loaded].SetActive(false);
            loaded = -1;
        }
        else if (mazeEmpty != null)
        {
            if (isRunning)
            {
                StopCoroutine(co);
            }
            if (!saveMaze)
            {
                Object.Destroy(mazeEmpty);
            }
            mazeEmpty.SetActive(false);
            saveMaze = false;
        }

        saveButton.interactable = true;
        saveButton.GetComponentInChildren<InputField>().interactable = true;
        faster.interactable = true;
        slower.interactable = true;
        regenerate.interactable = true;



        mazeNum++;

        float.TryParse(widthInput.text, out width);
        float.TryParse(heightInput.text, out height);

        mazeEmpty = new GameObject("Maze " + mazeNum);
        Transform maze = new GameObject("Maze").transform;
        maze.parent = mazeEmpty.transform;

        saveName.text = "Maze " + mazeNum;

        distance = mazeCell.transform.localScale.x;

        if (width % 2 == 0)
        {
            posX = (distance / 2) - ((width / 2) * distance);
        }
        else
        {
            posX = (distance / 2) - ((width / 2) * distance) - distance;
        }

        if (height % 2 == 0)
        {
            posY = (distance / 2) - ((height / 2) * distance);
        }
        else
        {
            posY = (distance / 2) - ((height / 2) * distance) - distance;
        }

        float posYOriginal = posY;
        float posXOriginal = posX;
        Transform firstCell = null;

        for (int w = 0; w < width; w++)
        {
            for (int h = 0; h < height; h++)
            {
                if (h == 0)
                {
                    firstCell = Instantiate(mazeCell, new Vector3(posX, posY, 0), transform.rotation).transform;
                    firstCell.transform.parent = maze;
                }
                else
                {
                    (Instantiate(mazeCell, new Vector3(posX, posY, 0), transform.rotation)).transform.parent = maze;
                }
                posY += distance;
            }
            posX += distance;
            posY = posYOriginal;
        }

        Transform walls = new GameObject("Walls").transform;
        walls.parent = mazeEmpty.transform;

        posX = posXOriginal;
        posY = posYOriginal;

        for (int h = 0; h <= height; h++)
        {
            for (int w = 0; w < width; w++)
            {
                if (h != height)
                {
                    (Instantiate(mazeWall, new Vector3(posX - (distance / 2), posY, 0), transform.rotation) as GameObject).transform.parent = walls.transform;
                }
                (Instantiate(mazeWall, new Vector3(posX, posY - (distance / 2), 0), Quaternion.Euler(0, 0, 90)) as GameObject).transform.parent = walls.transform;
                posX += distance;
            }
            if (h != height)
            {
                (Instantiate(mazeWall, new Vector3(posX - (distance / 2), posY, 0), transform.rotation) as GameObject).transform.parent = walls.transform;
            }
            posY += distance;
            posX = posXOriginal;
        }

        float size;
        if (height < width)
        {
            size = width;
        }
        else
        {
            size = height;
        }
        scale = 20/(size/10);
        mazeEmpty.transform.localScale = new Vector3(scale, scale, 1);

        CreateMaze();
    }

    public void CreateMaze()
    {
        if (isRunning)
        {
            StopCoroutine(co);
        }
        if (width > 40 || height > 40)
        {
            zoomIn.interactable = true;
        }
        else
        {
            zoomIn.interactable = false;
        }

        currentMazeCells = new List<GameObject>();
        currentMazeWalls = new List<Transform>();

        Transform currentMaze = mazeEmpty.transform.Find("Maze");
        Transform currentWalls = mazeEmpty.transform.Find("Walls");

        foreach (Transform cell in currentMaze)
        {
            cell.GetComponent<SpriteRenderer>().color = Color.white;
            currentMazeCells.Add(cell.gameObject);
        }

        foreach (Transform wall in currentWalls)
        {
            wall.gameObject.SetActive(true);
            currentMazeWalls.Add(wall);
        }

        co = StartCoroutine(GenerateMazeIterativeDepthFirst());
    }

    IEnumerator GenerateMazeIterativeDepthFirst()
    {
        isRunning = true;
        visited = new Stack<GameObject>();
        finishedMaze = new Stack<GameObject>();

        int startCell = Random.Range(0, currentMazeCells.Count);
        currentCell = currentMazeCells[startCell];

        sec = .01f;
        int nextCellNum;

        visited.Push(currentCell);
        GetNeighbours(currentCell);

        currentCell.GetComponent<SpriteRenderer>().color = Color.blue;
        yield return new WaitForSeconds(sec);

        while (finishedMaze.Count != currentMazeCells.Count)
        {
            nextCellNum = Random.Range(0, neigbours.Count);
            GameObject nextCell = neigbours[nextCellNum];
            RemoveWall(currentCell, nextCell);

            currentCell = nextCell;
            visited.Push(currentCell);
            GetNeighbours(currentCell);

            currentCell.GetComponent<SpriteRenderer>().color = Color.blue;
            yield return new WaitForSeconds(sec);
            

            if (neigbours.Count == 0)
            {
                while (neigbours.Count == 0 && finishedMaze.Count != currentMazeCells.Count)
                {
                    currentCell = visited.Peek();
                    finishedMaze.Push(visited.Pop());

                    GetNeighbours(currentCell);
                    currentCell.GetComponent<SpriteRenderer>().color = Color.cyan;
                    yield return new WaitForSeconds(sec);
                }
            }
        }
        isRunning = false;
        slower.interactable = false;
        ZoomOut();
        faster.interactable = false;
    }

    public void RemoveWall(GameObject currentCell, GameObject nextCell)
    {
        Vector3 position = currentCell.transform.localPosition;

        if (currentCell.transform.localPosition.x == nextCell.transform.localPosition.x)
        {
            if (currentCell.transform.localPosition.y > nextCell.transform.localPosition.y)
            {
                position.y -= distance / 2;
            }
            else
            {
                position.y += distance / 2;
            }
        }
        else if (currentCell.transform.localPosition.x > nextCell.transform.localPosition.x)
        {
            position.x -= distance / 2;
        }
        else
        {
            position.x += distance / 2;
        }

        foreach (Transform wall in currentMazeWalls)
        {
            if (wall.localPosition == position)
            {
                wall.gameObject.SetActive(false);
            }
        }
    }

    public void GetNeighbours(GameObject currentCell)
    {
        neigbours = new List<GameObject>();

        foreach (GameObject cell in currentMazeCells)
        {
            Vector3 position = currentCell.transform.localPosition;

            if (cell.transform.localPosition.x == position.x + distance || cell.transform.localPosition.x == position.x - distance)
            {
                if (cell.transform.localPosition.y == position.y)
                {
                    if (!visited.Contains(cell) && !finishedMaze.Contains(cell))
                    {
                        neigbours.Add(cell);
                    }
                }
            }
            else if (cell.transform.localPosition.y == position.y + distance || cell.transform.localPosition.y == position.y - distance)
            {
                if (cell.transform.localPosition.x == position.x)
                {
                    if (!visited.Contains(cell) && !finishedMaze.Contains(cell))
                    {
                        neigbours.Add(cell);
                    }
                }
            }
        }
    }

    public void SpeedUp()
    {

        if(sec < 0.001)
        {
            sec /= 2;
            faster.interactable = false;
        }
        else
        {
            sec /= 2;
            if (!slower.interactable)
            {
                slower.interactable = true;
            }
        }
    }

    public void SlowDown()
    {
        if (sec < .6)
        {
            sec *= 2;
            if (!faster.interactable)
            {
                faster.interactable = true;
            }
        }
        else
        {
            sec *= 2;
            slower.interactable = false;
        }
    }

    public void ZoomIn()
    {
        isZoomed = true;
        mazeEmpty.transform.localScale = new Vector3(10, 10, 1);
        zoomIn.interactable = false;
        zoomOut.interactable = true;
        sec = 0.1f;
    }

    public void ZoomOut()
    {
        isZoomed = false;
        mazeEmpty.transform.localScale = new Vector3(scale, scale, 1);
        mazeCam.transform.position = new Vector3(0, 0, -100);
        zoomOut.interactable = false;
        if (isRunning)
        {
            zoomIn.interactable = true;
        }
        sec = 0.01f;
    }
}

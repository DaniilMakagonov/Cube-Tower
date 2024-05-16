using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = System.Random;

public class GameController : MonoBehaviour
{
  private CubePos _nowCube = new(0, 1, 0);
  private readonly List<Vector3> _allCubePositions = new()
  {
    new Vector3(0,0,0),
    new Vector3(1,0,1),
    new Vector3(1,0,0),
    new Vector3(1,0,-1),
    new Vector3(-1,0,1),
    new Vector3(-1,0,0),
    new Vector3(-1,0,-1),
    new Vector3(0,0,1),
    new Vector3(0,0,-1),
    new Vector3(0,1,0)
  };

  private Rigidbody _allCubesRb;
  private bool _isLose, _firstCube;
  private Coroutine _coroutine;
  private float _camMoveToYPosition, _camMoveSpeed = 2f;
  private Transform _mainCam;
  private int _prevCountMaxHorizontal;
  private Color _toCameraColor;
  private AudioSource _audioSource;
  private Camera _main;
  private readonly List<GameObject> _possibleCubesToCreate = new();
  private readonly List<int> _pricesOfCubes = new() {0, 5, 10, 15, 20, 30, 50, 70, 100};

  public Transform cubeToPlace;
  public float cubeChangePlaceSpeed = .5f;
  public GameObject  allCubes, vfx;
  public GameObject[] canvasStartPage;
  public Color[] bgColors;
  public Text scoreText;
  public GameObject[] cubesToCreate;

  private void SpawnPositions()
  {
    List<Vector3> positions = new();
    if (_nowCube.X + 1 != cubeToPlace.position.x && _nowCube.Y > 0 && !_allCubePositions.Contains(new Vector3(_nowCube.X + 1, _nowCube.Y, _nowCube.Z)))
      positions.Add(new Vector3(_nowCube.X + 1, _nowCube.Y, _nowCube.Z));
    if (_nowCube.X - 1 != cubeToPlace.position.x && _nowCube.Y > 0 && !_allCubePositions.Contains(new Vector3(_nowCube.X - 1, _nowCube.Y, _nowCube.Z)))
      positions.Add(new Vector3(_nowCube.X - 1, _nowCube.Y, _nowCube.Z));
    if (_nowCube.Y + 1 != cubeToPlace.position.y && _nowCube.Y + 1 > 0 && !_allCubePositions.Contains(new Vector3(_nowCube.X, _nowCube.Y + 1, _nowCube.Z)))
      positions.Add(new Vector3(_nowCube.X, _nowCube.Y + 1, _nowCube.Z));
    if (_nowCube.Y - 1 != cubeToPlace.position.y && _nowCube.Y - 1 > 0 && !_allCubePositions.Contains(new Vector3(_nowCube.X, _nowCube.Y - 1, _nowCube.Z)))
      positions.Add(new Vector3(_nowCube.X, _nowCube.Y - 1, _nowCube.Z));
    if (_nowCube.Z + 1 != cubeToPlace.position.z && _nowCube.Y > 0 && !_allCubePositions.Contains(new Vector3(_nowCube.X, _nowCube.Y, _nowCube.Z + 1)))
      positions.Add(new Vector3(_nowCube.X, _nowCube.Y, _nowCube.Z + 1));
    if (_nowCube.Z - 1 != cubeToPlace.position.z && _nowCube.Y > 0 && !_allCubePositions.Contains(new Vector3(_nowCube.X, _nowCube.Y, _nowCube.Z - 1)))
      positions.Add(new Vector3(_nowCube.X, _nowCube.Y, _nowCube.Z - 1));

    if (positions.Count > 0)
      cubeToPlace.position = positions[UnityEngine.Random.Range(0, positions.Count)];
    else
      _isLose = true;
  }

  private void MoveCameraChangeBg()
  {
    int maxX = 0, maxY = 0, maxZ = 0, maxHor;
    foreach (var pos in _allCubePositions)
    {
      maxX = Math.Max(Math.Abs(Convert.ToInt32(pos.x)), maxX);
      maxY = Math.Max(Math.Abs(Convert.ToInt32(pos.y)), maxY);
      maxZ = Math.Max(Math.Abs(Convert.ToInt32(pos.z)), maxZ);
    }

    if (PlayerPrefs.GetInt("score") < maxY - 1)
    {
      PlayerPrefs.SetInt("score", maxY - 1);
    }

    scoreText.text = "<size=35>BEST: </size>" + PlayerPrefs.GetInt("score") + "\n<size=35>now: </size>" + (maxY - 1);
    
    _camMoveToYPosition = 6f + _nowCube.Y - 1f;
    maxHor = Math.Max(maxX, maxZ);
    
    if (maxHor % 3 == 0 && maxHor != _prevCountMaxHorizontal)
    {
      _mainCam.localPosition += new Vector3(0, 0, -3f);
      _prevCountMaxHorizontal = maxHor;
    }

    if (maxY >= 7)
      _toCameraColor = bgColors[2];
    else if (maxY >= 5)
      _toCameraColor = bgColors[1];
    else if (maxY >= 3)
      _toCameraColor = bgColors[0];
  }

  private void Start()
  {
    for (int i = 0; i < _pricesOfCubes.Count; ++i)
      if (PlayerPrefs.GetInt("score") >= _pricesOfCubes[i])
        _possibleCubesToCreate.Add(cubesToCreate[i]);
    _main = Camera.main;
    _audioSource = GetComponent<AudioSource>();
    _toCameraColor = _main!.backgroundColor;
    _mainCam = _main.transform;
    _camMoveToYPosition = 6f + _nowCube.Y - 1f;
    _allCubesRb = allCubes.GetComponent<Rigidbody>();
    _coroutine = StartCoroutine(ShowCubePlace());
    scoreText.text = "<size=35>BEST: </size>" + PlayerPrefs.GetInt("score") + "\n<size=35>now: </size>0";
  }

  private void Update()
  {
    if (cubeToPlace && allCubes && (Input.GetMouseButtonDown(0) || Input.touchCount > 0) && !EventSystem.current.IsPointerOverGameObject())
    {
#if !UNITY_EDITOR
      if (Input.GetTouch(0).phase != TouchPhase.Began)
        return;
#endif

      if (!_firstCube)
      {
        _firstCube = true;
        foreach (GameObject el in canvasStartPage)
        {
          Destroy(el);
        }
      }
      
      GameObject newCube = Instantiate(
        _possibleCubesToCreate[UnityEngine.Random.Range(0, _possibleCubesToCreate.Count)],
        cubeToPlace.position,
        Quaternion.identity);
      
      newCube.transform.SetParent(allCubes.transform);
      _nowCube.SetVector(cubeToPlace.position);
      _allCubePositions.Add(_nowCube.GetVector());

      GameObject vfxObj = Instantiate(vfx, _nowCube.GetVector(), Quaternion.identity) as GameObject;
      Destroy(vfxObj, 1f);
      if (PlayerPrefs.GetString("music") != "No")
        _audioSource.Play();

      _allCubesRb.isKinematic = true;
      _allCubesRb.isKinematic = false;
      
      SpawnPositions();
      MoveCameraChangeBg();
    }

    if (!_isLose && _allCubesRb.velocity.magnitude > .1f)
    {
      Destroy(cubeToPlace.gameObject);
      _isLose = true;
      StopCoroutine(_coroutine);
    }

    _mainCam.localPosition = Vector3.MoveTowards(_mainCam.localPosition,
      new Vector3(_mainCam.localPosition.x, _camMoveToYPosition, _mainCam.localPosition.z),
      _camMoveSpeed * Time.deltaTime);

    if (_main!.backgroundColor != _toCameraColor)
      _main.backgroundColor = Color.Lerp(_main.backgroundColor, _toCameraColor, Time.deltaTime * 2);
  }

  private IEnumerator ShowCubePlace()
  {
    while (true)
    {
      SpawnPositions();
      yield return new WaitForSeconds(cubeChangePlaceSpeed);
    }
  }
}

internal struct CubePos
{
  public int X, Y, Z;

  public CubePos(int x, int y, int z)
  {
    this.X = x;
    this.Y = y;
    this.Z = z;
  }

  public Vector3 GetVector()
  {
    return new Vector3(X, Y, Z);
  }

  public void SetVector(Vector3 vector)
  {
    X = Convert.ToInt32(vector.x);
    Y = Convert.ToInt32(vector.y);
    Z = Convert.ToInt32(vector.z);
  }
}
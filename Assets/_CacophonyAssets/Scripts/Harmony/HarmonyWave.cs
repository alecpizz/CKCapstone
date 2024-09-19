using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: Ryan
/// Description: Projects a harmony wave and detects things that it hits
/// </summary>
public class HarmonyWave : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] Vector3 _waveDirection;
    [SerializeField] float _waveLength;
    Vector3 _lastRayEndLocation;
    [Space]

    [Header("Curve")] 
    [SerializeField] int points;
    [SerializeField] float amp;
    [Space]

    [Header("Layers")]
    [SerializeField] LayerMask _stopWaveAtLayers;
    [SerializeField] LayerMask _harmonizeWithLayers;
    [Space]

    [Header("Components")]
    [SerializeField] LineRenderer _lr;
    [SerializeField] GameObject SelfPrefab;
    [Space]

    [Header("Harmony Type")]
    [SerializeField] HarmonizationType _harmonyTypeOfWave;

    //ChildWaves
    List<HarmonyWave> _childWaves = new();
    HarmonyWave _parentWave;
    int _childID = 0;
    //Prevents Infinite Loops
    int _maxChildID = 10;
    GameObject _currentInteractionObject;
    GameObject _lastHitInteractionObject;

    
    List<IHarmonizable> storedHarmonizables = new();

    // Start is called before the first frame update
    void Start()
    {
        if (RoomSolvedDisable()) return;
        GameplayManagers.Instance.Harmonizer.AddHarmonyWave(this);
        SubscribeToEvents();

        ChangeHarmonyVisuals(_harmonyTypeOfWave);
        ProjectHarmonyWave();
        StartCoroutine(Curve());



    }

    private void SubscribeToEvents()
    {
        GameplayManagers.Instance.Harmonizer.GetVisualizeAllWavesEvent().AddListener(ProjectHarmonyWave);
    }


    /// <summary>
    /// Projects the harmony wave with nothing passed in, uses _waveDirection
    /// </summary>
    public void ProjectHarmonyWave()
    {
        VisualizeWave(_waveDirection);
    }
    /// <summary>
    /// Projects the harmony wave with a specific direction
    /// </summary>
    /// <param name="specifiedDirection"></param>
    public void ProjectHarmonyWave(Vector3 specifiedDirection)
    {
        VisualizeWave(specifiedDirection);
        _waveDirection = specifiedDirection;
    }

    /// <summary>
    /// Projects the visual of the harmony wave until it hits something that should stop it
    /// </summary>
    private void VisualizeWave(Vector3 specifiedDirection)
    {
        if (RoomSolvedDisable()) return;
        RemoveHarmonizationWithEverything();

        //Prevents infinite loops by setting a hard cap for the amount of waves that can be made from 1 wave
        bool canCreateWave = true;
        if (_maxChildID <= _childID)
            canCreateWave = false;

        
        RaycastHit[] visualHarmonyArray = Physics.RaycastAll(transform.position, specifiedDirection, _waveLength, _stopWaveAtLayers);
        //if (Physics.Raycast(transform.position, specifiedDirection, out rayHit, _waveLength, _stopWaveAtLayers))
        if (visualHarmonyArray.Length != 0)
        {
            //Debug.Log("Visualizing Wave");
            foreach (RaycastHit currentVisualRay in visualHarmonyArray)
            {
                //Checks if the last thing we hit is what we are currently hitting
                if (_lastHitInteractionObject != null && currentVisualRay.collider.gameObject != _lastHitInteractionObject)
                {
                    RemoveAllChildWaves();
                    //Checks if the last thing we hit is the other side of a portal that we are hitting
                    /*if (_lastHitInteractionObject.GetComponentInParent<WaveTeleporter>() != null &&
                        currentVisualRay.collider.gameObject == _lastHitInteractionObject.GetComponentInParent<WaveTeleporter>().
                        FindOtherTeleporter(_lastHitInteractionObject))*/
                    if (currentVisualRay.collider.gameObject == _lastHitInteractionObject)
                    {
                        return;
                    }
                    
                }
                
                //Stops the wave at the thing we hit
                SetEndOfVisualWave(currentVisualRay.point);
                if (!canCreateWave) return;

                if (currentVisualRay.collider.gameObject == _currentInteractionObject)
                    return;

                //Hits an object to reflect off of
                if (HitReflector(currentVisualRay))
                    return;
                //Hits an object that changes wave type
                if (HitWaveChanger(currentVisualRay, specifiedDirection))
                    return;
                //Hits a teleporter
                if (HitTeleporter(currentVisualRay, specifiedDirection))
                    return;
                //Hits a wall
                if (HitWall(currentVisualRay))
                    return;
            }
            
        }

        //Harmony Wave hit nothing
        SetEndOfVisualWave(transform.position + new Vector3(specifiedDirection.x * _waveLength, 0, specifiedDirection.z * _waveLength));
        _lastHitInteractionObject = null;
        if (_childID == 0)
            RemoveAllChildWaves();
    }

    #region WaveDetectionObjects
    private bool HitReflector(RaycastHit currentVisualRay)
    {
        if (currentVisualRay.collider.gameObject.GetComponentInParent<ReflectHarmony>())
        {
            if (!HitEnemy(currentVisualRay))
                return false;
                    
                
            ReflectOff(currentVisualRay.collider.gameObject);

            _lastHitInteractionObject = currentVisualRay.collider.gameObject;
            return true;
        }
        return false;
    }

    private bool HitWaveChanger(RaycastHit currentVisualRay, Vector3 specifiedDirection)
    {
        //Hits an object to change wave type
        if (currentVisualRay.collider.gameObject.GetComponentInParent<WaveChanger>())
        {
            //Creates a new wave at the point of the wave changer with the wave changers wave type
            CreateVisualChildWave(currentVisualRay.collider.gameObject, specifiedDirection,
                currentVisualRay.collider.gameObject.GetComponentInParent<WaveChanger>().GetChangeType());
            _lastHitInteractionObject = currentVisualRay.collider.gameObject;
            return true;
        }
        return false;
    }

    private bool HitTeleporter(RaycastHit currentVisualRay, Vector3 specifiedDirection)
    {
        if (currentVisualRay.collider.gameObject.GetComponentInParent<WaveTeleporter>())
        {
            //Debug.Log("Added Teleporter Wave");
            CreateVisualChildWave(currentVisualRay.collider.gameObject.GetComponentInParent<WaveTeleporter>()
                .FindOtherTeleporter(currentVisualRay.collider.gameObject), specifiedDirection, _harmonyTypeOfWave);
            

            _lastHitInteractionObject = currentVisualRay.collider.gameObject;

            return true;
        }
        return false;
    }

    private bool HitWall(RaycastHit currentVisualRay)
    {
        //Harmony Wave hit a wall and should stop
        if (currentVisualRay.collider.gameObject.CompareTag("Wall"))
        {
            _lastHitInteractionObject = null;
            return true;
        }
        return false;
    }

    private bool HitEnemy(RaycastHit currentVisualRay)
    {
        //If I'm hitting an enemy make sure its the correct harmony type
        if (currentVisualRay.collider.GetComponent<IHarmonizable>() != null)
        {
            
            storedHarmonizables.Add(currentVisualRay.collider.GetComponent<IHarmonizable>());
            GameplayManagers.Instance.Harmonizer.AddHarmonized(currentVisualRay.collider.GetComponent<IHarmonizable>(), _harmonyTypeOfWave);

            if (currentVisualRay.collider.GetComponent<IHarmonizable>().GetHarmonizationType() != _harmonyTypeOfWave)
                return false;
        }
            
            

        if (!currentVisualRay.collider.gameObject.GetComponentInParent<ReflectHarmony>().GetCanReflect())
            return false;

        return true;
    }
    #endregion

    /// <summary>
    /// Finds the ending point of the visual wave
    /// </summary>
    /// <param name="endPoint"></param>
    private void SetEndOfVisualWave(Vector3 endPoint)
    {
        _lastRayEndLocation = endPoint;
        SetWaveDirection((endPoint - transform.position).normalized);
    }

    /// <summary>
    /// Reflects off a reflection Object
    /// </summary>
    /// <param name="reflectObject"></param>
    private void ReflectOff(GameObject reflectObject)
    {
        //Checks if we already hit the object or if the room is solved
        if (_lastHitInteractionObject == reflectObject || GameplayManagers.Instance.Room.GetRoomSolved())
            return;

        //Creates a child wave for each direction that it reflects in
        foreach(Vector3 harmonyDirection in reflectObject.GetComponentInParent<ReflectHarmony>().GetReflectDirection())
        {
            CreateVisualChildWave(reflectObject, harmonyDirection,_harmonyTypeOfWave);
        }
        
    }


    /// <summary>
    /// Creates a new wave that is a child of this wave
    /// </summary>
    /// <param name="hitObject"></param>
    /// <param name="direction"></param>
    private HarmonyWave CreateVisualChildWave(GameObject hitObject, Vector3 direction, HarmonizationType harmonyType)
    {
        if(FindObjectsOfType<HarmonyWave>().Length > 50)
        {
            return null;
        }
        HarmonyWave recentChildWave = Instantiate(SelfPrefab, hitObject.transform.position, Quaternion.identity).GetComponentInParent<HarmonyWave>();
        if (recentChildWave == null)
            return null;

        recentChildWave.SetInteractionObject(hitObject);
        recentChildWave.SetChildID(_childID + 1);
        recentChildWave.SetParentWave(this);
        recentChildWave.ChangeHarmonyType(harmonyType);

        recentChildWave.VisualizeWave(direction);

        _childWaves.Add(recentChildWave);
        return recentChildWave;
    }

    /// <summary>
    /// Deletes any child waves that exist
    /// </summary>
    public void RemoveAllChildWaves()
    {
        
        if (_childWaves.Count > 0)
        {
            int childWaveCount = _childWaves.Count;
            for (int i = 0; i < childWaveCount; i++)
                _childWaves[0].RemoveAllChildWaves();
            
        }

        if (_childID != 0)
        {
            GameplayManagers.Instance.Harmonizer.RemoveHarmonyWave(this);
            if (_parentWave != null)
                _parentWave.GetChildWaves().Remove(this);
            RemoveHarmonizationWithEverything();
            GameplayManagers.Instance.Harmonizer.GetVisualizeAllWavesEvent().RemoveListener(ProjectHarmonyWave);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Deharmonizes everythign that this is harmonized with
    /// </summary>
    private void RemoveHarmonizationWithEverything()
    {
        foreach (IHarmonizable harmonizable in storedHarmonizables)
            GameplayManagers.Instance.Harmonizer.RemoveHarmonized(harmonizable,_harmonyTypeOfWave);
        storedHarmonizables = new();
    }

    /// <summary>
    /// Changes the harmony type of the wave
    /// </summary>
    /// <param name="newType"></param>
    public void ChangeHarmonyType(HarmonizationType newType)
    {
        _harmonyTypeOfWave = newType;
        ChangeHarmonyVisuals(newType);
    }

    /// <summary>
    /// Makes the visuals for the wave curve
    /// </summary>
    /// <returns></returns>
    IEnumerator Curve()
    {
        while (true)
        {
            //Determines the math for the Y amp
            float tau = 2 * Mathf.PI;
            Vector3 xzFinish = GetComponent<HarmonyWave>().GetRayEndLocation();

            points = (int)Vector3.Distance(transform.position, xzFinish) * 5;
            _lr.positionCount = points;
            for (int currentPoint = 0; currentPoint < points; currentPoint++)
            {
                Vector3 xzStart = transform.position;
                float progress = (float)currentPoint / (points - 1);
                Vector3 xzPos = Vector3.Lerp(xzStart, xzFinish, progress);
                float xPos = Mathf.Lerp(0, tau, progress);
                float yPos = amp * Mathf.Sin(xPos + Time.timeSinceLevelLoad) + transform.position.y;
                _lr.SetPosition(currentPoint, new Vector3(xzPos.x, yPos, xzPos.z));
            }
            yield return null;
        }
    }

    private bool RoomSolvedDisable()
    {
        if (GameplayManagers.Instance.Room.GetRoomSolved())
        {
            //Destroy(gameObject);
            gameObject.SetActive(false);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Changes the material of the wave based on its type
    /// </summary>
    /// <param name="newType"></param>
    private void ChangeHarmonyVisuals(HarmonizationType newType)
    {
        _lr.material = GameplayManagers.Instance.Harmonizer.GetWaveMaterialDictionary()[newType];
    }

    public void SetInteractionObject(GameObject reflectObject)
    {
        _currentInteractionObject = reflectObject;
    }

    public void SetChildID(int id)
    {
        _childID = id;
    }

    public int GetChildID()
    {
        return _childID;
    }

    public List<HarmonyWave> GetChildWaves()
    {
        return _childWaves;
    }

    public void SetParentWave(HarmonyWave parent)
    {
        _parentWave = parent;
    }

    public HarmonyWave GetParentWave()
    {
        return _parentWave;
    }

    public Vector3 GetRayEndLocation()
    {
        return _lastRayEndLocation;
    }

    public LineRenderer GetLineRenderer()
    {
        return _lr;
    }
    public void SetWaveDirection(Vector3 waveDir)
    {
        _waveDirection = waveDir;
    }
}

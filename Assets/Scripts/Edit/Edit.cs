using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Edit
{
    public class Edit : MonoBehaviour
    {
        public Texture2D pricel;
        private Material _shablon;
        private Material _red;
        private AudioClip On, Off, Turn, Click;
        private GameObject _planePol;
        private GameObject _marker;
        private GameObject _pMarker;
        private GameObject _line;
        private GameObject _nObject;
        private GameObject _lineObject;
        float _lineX = 15f;
        private Camera _cam;
        RaycastHit[] _hits;
        [SerializeField] private int delta = 90;
        private bool _simMode = false;
        private readonly Texture[] _prev = new Texture[Setup.FiguresCount];
        private readonly Dictionary<int, Color> _colorArr = new Dictionary<int, Color>();
        private bool _rotating = false;
        private GameObject _colorPanel;
        private GameObject _figurePanel;
        private GameObject _infoPanel;
        private GameObject _weaponPanel;
        private int _cpu;
        private int _health;
        private int _mass;
        private Button _btnLeft;
        private Button _btnRight;
        private Button _btnDelete;
        private int _buttonIndex = 1;
        private AudioSource _audio;

        private void Start()
        {
            _audio = GetComponent<AudioSource>();
            Setup.Start();
            _colorPanel = GameObject.Find("ColorPanel");
            _infoPanel = GameObject.Find("InfoPanel");

            _shablon = Resources.Load("Materials/Shablon", typeof(Material)) as Material;
            _red = Resources.Load("Materials/Red", typeof(Material)) as Material;
            On = Resources.Load("Audio/On", typeof(AudioClip)) as AudioClip;
            Off = Resources.Load("Audio/Off", typeof(AudioClip)) as AudioClip;
            Turn = Resources.Load("Audio/Turn", typeof(AudioClip)) as AudioClip;
            Click = Resources.Load("Audio/Click", typeof(AudioClip)) as AudioClip;
            _planePol = Resources.Load("Models/Edit/PlanePol", typeof(GameObject)) as GameObject;
            _marker = Resources.Load("Models/Edit/Marker", typeof(GameObject)) as GameObject;
            _line = Resources.Load("Models/Edit/Line", typeof(GameObject)) as GameObject;
            Setup.Pause = false;

            _cam = Camera.main;

            for (int z = 0; z < 31; z++)
            for (int x = 0; x < 31; x++)
            {
                var no = Instantiate(_planePol, GameObject.Find("Pole").transform, true);
                no.transform.position = new Vector3(x, 0, z);
                no.name = "PlaneP";
            }
            Setup.LoadBlocks();
            LoadInfo();
            InfoPanelUpdate();
            transform.LookAt(GameObject.Find("Figures").transform);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            MadeFigureShablon();
            CreateFigurePanel();
            CreateColorPanel();
            CreateWeaponPanel();
        }

        private void LoadInfo()
        {
            _cpu = 0;
            _health = 0;
            _mass = 0;
            foreach (Transform child in GameObject.Find("Figures").transform)
            {
                _cpu += Setup.Figures[Setup.NameToInt(child.name)].Cpu;
                _health += Setup.Figures[Setup.NameToInt(child.name)].Health;
                _mass += Setup.Figures[Setup.NameToInt(child.name)].Mass;
            }
        }

        private void PlayAudio(AudioClip x)
        {
            _audio.clip = x;
            _audio.Play();
        }

        private void CreateWeaponPanel()
        {
            var btnOk = GameObject.Find("OkButton").GetComponent<Button>();
            _btnRight = GameObject.Find("RightButton").GetComponent<Button>();
            _btnLeft = GameObject.Find("LeftButton").GetComponent<Button>();
            _btnDelete = GameObject.Find("DeleteButton").GetComponent<Button>();
            _weaponPanel = GameObject.Find("WeaponPanel");

            btnOk.onClick.AddListener(OkButtonOnClick);
            _btnRight.onClick.AddListener(RightButtonOnClick);
            _btnLeft.onClick.AddListener(LeftButtonOnClick);
            _btnDelete.onClick.AddListener(DeleteButtonOnClick);

            var btn1 = GameObject.Find("GridItem1").transform.Find("WeaponItem").GetComponent<Button>();
            btn1.onClick.AddListener(() => GridItemOnClick(btn1, 1));
            var btn2 = GameObject.Find("GridItem2").transform.Find("WeaponItem").GetComponent<Button>();
            btn2.onClick.AddListener(() => GridItemOnClick(btn2, 2));
            var btn3 = GameObject.Find("GridItem3").transform.Find("WeaponItem").GetComponent<Button>();
            btn3.onClick.AddListener(() => GridItemOnClick(btn3, 3));

            for (int x = 0; x < 3; x++)
            {
                PutOnWeaponPanel(x + 1, Setup.LA[x]);
            }

            EditButtonsUpdate();
            _weaponPanel.SetActive(false);
        }

        private void EditButtonsUpdate()
        {
            _btnLeft.gameObject.SetActive(_buttonIndex != 1);
            _btnRight.gameObject.SetActive(_buttonIndex != 3);
            _btnDelete.gameObject.SetActive(Setup.LA[_buttonIndex - 1] > -1);
        }

        private void PutOnWeaponPanel(int index, int idFigure)
        {
            var www = false;
            if (!_weaponPanel.activeSelf)
            {
                _weaponPanel.SetActive(true);
                www = true;
            }
            var g = GameObject.Find("GridItem" + index);
            var I = g.transform.Find("Image").GetComponent<Image>();
            var btn = g.transform.Find("WeaponItem").GetComponent<Button>();
            if (idFigure < 0)
            {
                I.sprite = null;
                I.color = new Color32(255, 255, 255, 0);
                btn.transform.Find("Text2").GetComponent<Text>().text = "";
            }
            else
            {
                RuntimePreviewGenerator.BackgroundColor = new Color(0, 0, 0, 0);
                var prev = RuntimePreviewGenerator.GenerateModelPreview(Setup.Figures[idFigure].GO.transform, 100, 100, false) as Texture;
                I.sprite = Sprite.Create((Texture2D)prev, new Rect(0, 0, 100, 100), new Vector2(0.5f, 0.5f));
                I.color = new Color32(255, 255, 255, 255);
                btn.transform.Find("Text2").GetComponent<Text>().text = Setup.Figures[idFigure].Name;
            }
            Setup.LA[index - 1] = idFigure;
            if (www) _weaponPanel.SetActive(false);
        }

        private void GridItemOnClick(Button btn, int x)
        {
            PlayAudio(Click);
            var editButtons = GameObject.Find("EditButtons");
            editButtons.transform.position = new Vector2(btn.transform.position.x, editButtons.transform.position.y);
            _buttonIndex = x;
            EditButtonsUpdate();
        }

        private void DeleteButtonOnClick()
        {
            var value = Setup.LA[_buttonIndex - 1];

            PutOnWeaponPanel(_buttonIndex, -1);

            _btnDelete.gameObject.SetActive(false);
            PlayAudio(Click);
            if (value < 0) return;
            print(value);
            foreach (Transform child in GameObject.Find("Figures").transform)
            {
                if (Setup.Figures[value].Name == child.name)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void RightButtonOnClick()
        {
            PlayAudio(Click);
            var editButtons = GameObject.Find("EditButtons");
            var position = editButtons.transform.position;
            position = new Vector2(position.x + 196, position.y);
            editButtons.transform.position = position;
            _buttonIndex++;
            GameObject.Find("GridItem" + (_buttonIndex).ToString()).transform.Find("WeaponItem").GetComponent<Button>().Select();

            var value1 = Setup.LA[_buttonIndex - 1];
            var value2 = Setup.LA[_buttonIndex - 2];

            PutOnWeaponPanel(_buttonIndex, value2);
            PutOnWeaponPanel(_buttonIndex - 1, value1);
            EditButtonsUpdate();

        }

        private void LeftButtonOnClick()
        {
            PlayAudio(Click);
            var editButtons = GameObject.Find("EditButtons");
            var position = editButtons.transform.position;
            position = new Vector2(position.x - 196, position.y);
            editButtons.transform.position = position;
            _buttonIndex--;
            GameObject.Find("GridItem" + (_buttonIndex).ToString()).transform.Find("WeaponItem").GetComponent<Button>().Select();

            var value1 = Setup.LA[_buttonIndex - 1];
            var value2 = Setup.LA[_buttonIndex];

            PutOnWeaponPanel(_buttonIndex, value2);
            PutOnWeaponPanel(_buttonIndex + 1, value1);
            EditButtonsUpdate();

        }

        private void OkButtonOnClick()
        {
            TriggerPanel(_weaponPanel);
        }

        private void CreateFigurePanel()
        {
            RuntimePreviewGenerator.BackgroundColor = new Color(0, 0, 0, 1);
            RuntimePreviewGenerator.OrthographicMode = true;
            RuntimePreviewGenerator.PreviewDirection = new Vector3(-0.7f, -0.4f, -1f);
            RuntimePreviewGenerator.Padding = 0.1f;
            for (int i = 0; i < Setup.FiguresCount; i++)
            {
                Setup.Figures[i].GO.transform.GetComponent<MeshRenderer>().sharedMaterials[0].color = new Color32(58, 90, 186, 255);
                _prev[i] = RuntimePreviewGenerator.GenerateModelPreview(Setup.Figures[i].GO.transform, 200, 200, false) as Texture;
                if (Setup.Figures[i].GO.name == "Wheel1")
                {
                    var papa2 = new GameObject("papa2");
                    var child = GameObject.Instantiate(Setup.Figures[i].GO, papa2.transform, true);
                    child.transform.name = "Child";
                    child.transform.Rotate(0, 180, 90);
                    _prev[i] = RuntimePreviewGenerator.GenerateModelPreview(papa2.transform, 200, 200, false) as Texture;
                    Destroy(papa2);
                }
                var button = Instantiate(GameObject.Find("FigureItem"), GameObject.Find("Content").transform);
                button.name = i.ToString();
                button.transform.Find("Panel1").Find("Image").GetComponent<Image>().sprite = Sprite.Create((Texture2D)_prev[i], new Rect(0, 0, 200, 200), new Vector2(0.5f, 0.5f));
                button.GetComponent<Button>().onClick.AddListener(() => ButtonClick(button, i));
                button.transform.Find("Panel2").Find("Text").GetComponent<Text>().text = Setup.Figures[i].Cpu.ToString() + " CPU";
                button.transform.Find("Panel3").Find("Text").GetComponent<Text>().text =
                    Setup.Figures[i].Name + "\r\n" + Setup.Figures[i].Faces + " Faces";
            }
            Destroy(GameObject.Find("FigureItem"));
            _figurePanel = GameObject.Find("FigurePanel");
            var c = GameObject.Find("Content");
            var s = c.transform.childCount / 9;
            if ((c.transform.childCount % 9) > 0) s++;
            c.GetComponent<RectTransform>().sizeDelta = new Vector2(0, s * 310 + 10);
            _figurePanel.SetActive(false);
        }

        private int ButtonClick(GameObject btn, int x)
        {
            Setup.IdCube = Convert.ToInt32(btn.name);
            _figurePanel.SetActive(false);
            _infoPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            PlayAudio(Click);
            Setup.Pause = false;
            MadeFigureShablon();
            return 0;
        }

        private void CreateColorPanel()
        {
            var panel = GameObject.Find("ColorGrid").transform;
            for (int i = 0; i < 32; i++)
            {

                var button = Instantiate(GameObject.Find("ColorItem"), panel);
                button.name = i.ToString();
                var x = button.GetComponent<Button>().colors;
                var color = Setup.Palette[i];
                x.normalColor = color;
                color.a = 0.5f;
                x.highlightedColor = color;
                x.pressedColor = color;
                button.GetComponent<Button>().colors = x;
                button.GetComponent<Button>().onClick.AddListener(() => ButtonColorClick(button, i));
            }
            Destroy(GameObject.Find("ColorItem"));
            _colorPanel.SetActive(false);
        }

        private int ButtonColorClick(GameObject btn, int x)
        {
            Setup.MainColor = Setup.Palette[Convert.ToInt32(btn.name)];
            _colorPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            PlayAudio(Click);
            Setup.Pause = false;
            MadeFigureShablon();
            return 0;
        }

        private void End()
        {
            if (SceneManager.GetActiveScene().name == "Edit") Setup.SaveBlocks();
#if UNITY_EDITOR
            Setup.Figure1.color = new Color32(58, 90, 186, 255);
            UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
        }

        private void MadeFigureShablon()
        {
            Destroy(GameObject.Find("FigureShablon"));
            var eee = GameObject.Instantiate(Setup.Figures[Setup.IdCube].GO, GameObject.Find("FigureParent").transform);
            eee.transform.localPosition = Vector3.zero;// (0, 1.1f, -1);
            //eee.transform.localEulerAngles = new Vector3 (-45, 0, 0);
            eee.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            //eee.transform.GetComponent<MeshCollider> ().enabled = false;
            DisableMesh(eee.transform);
            eee.name = "FigureShablon";
            foreach (Transform child in eee.transform)
            {
                if (child.CompareTag("Plane"))
                    Destroy(child.gameObject);
            }
            Setup.Layered(eee, 8);
            //Setup.pokras (eee.transform, Setup.MainColor );
            RefreshFigureShablonColor(Setup.MainColor);
            eee.transform.Rotate(0, delta - 90, 0);
        }

        private static void RefreshFigureShablonColor(Color c)
        {
            //c=new Color(c.r/4, c.g/4,c.b/4);
            //c=c/4;
            Setup.pokras(GameObject.Find("FigureShablon").transform, c);
        }

        private IEnumerator Rotated(Vector3 angles)
        {
            var or = GameObject.Find("FigureShablon").transform;
            _rotating = true;
            var from = or.localRotation;
            var to = Quaternion.Euler(or.localEulerAngles + angles);
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 10;
                or.localRotation = Quaternion.Lerp(from, to, t);
                yield return null;
            }
            _rotating = false;
        }

        private static void DisableMesh(Transform x)
        {
            x.transform.GetComponent<MeshCollider>().enabled = false;
            foreach (Transform child in x)
            {
                if (!child.CompareTag("Plane"))
                    DisableMesh(child);
            }
        }

        private void InfoPanelUpdate()
        {
            GameObject.Find("Cpu").GetComponent<Text>().text = _cpu.ToString();
            GameObject.Find("CpuSlider").GetComponent<Slider>().value = _cpu;
            GameObject.Find("Health").GetComponent<Text>().text = _health.ToString();
            GameObject.Find("HealthSlider").GetComponent<Slider>().value = _health;
            GameObject.Find("Mass").GetComponent<Text>().text = _mass.ToString()+" kg";
        }

        private static void TriggerPanel(GameObject panel)
        {
            panel.SetActive(!panel.activeSelf);
            Cursor.visible = panel.activeSelf;
            Cursor.lockState = !panel.activeSelf ? CursorLockMode.Locked : CursorLockMode.None;
            Setup.Pause = panel.activeSelf;
            Input.ResetInputAxes();
        }

        private void Update()
        {
            _hits = Physics.RaycastAll(_cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f))).OrderBy(h => h.distance)
                .ToArray();
            if (Input.GetKeyDown(KeyCode.L) && (!Setup.Pause || (Setup.Pause && _weaponPanel.activeSelf)))
            {
                TriggerPanel(_weaponPanel);
                if (_weaponPanel.activeSelf)
                {
                    EditButtonsUpdate();
                    GameObject.Find("GridItem" + (_buttonIndex).ToString()).transform.Find("WeaponItem").GetComponent<Button>().Select();
                }
            }
            if (Input.GetKeyDown(KeyCode.C) && (!Setup.Pause || (Setup.Pause && _colorPanel.activeSelf)))
            {
                TriggerPanel(_colorPanel);
            }
            if (Input.GetKeyDown(KeyCode.Tab) && (!Setup.Pause || (Setup.Pause && _figurePanel.activeSelf)))
            {
                TriggerPanel(_figurePanel);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_figurePanel.activeSelf)
                {
                    TriggerPanel(_figurePanel);
                    return;
                }
                if (_colorPanel.activeSelf)
                {
                    TriggerPanel(_colorPanel);
                    return;
                }
                if (_weaponPanel.activeSelf)
                {
                    TriggerPanel(_weaponPanel);
                    return;
                }
            }

            if (Setup.Pause) return;
            if (Input.GetKeyDown(KeyCode.Comma))
            {
                float min = 1;
                foreach (GameObject g in GameObject.FindGameObjectsWithTag("Figure"))
                {
                    if (min > (float)g.GetComponent<Collider>().bounds.min.y) min = (float)g.GetComponent<Collider>().bounds.min.y;
                }
                if (min < 1) return;
                foreach (Transform ch in GameObject.Find("Figures").transform)
                {
                    var position = ch.position;
                    position = new Vector3(position.x, position.y - 1, position.z);
                    ch.position = position;
                    PlayAudio(Turn);
                }
            }
            if (Input.GetKeyDown(KeyCode.Period))
            {
                foreach (Transform ch in GameObject.Find("Figures").transform)
                {
                    var position = ch.position;
                    position = new Vector3(position.x, position.y + 1, position.z);
                    ch.position = position;
                    PlayAudio(Turn);
                }
            }

            if ((Input.GetAxis("Mouse ScrollWheel") < 0) && !_rotating)
            {
                if (Setup.IntToName(Setup.IdCube) == "Wheel1") return;
                delta -= 90;
                if (delta == -90) delta = 270;
                PlayAudio(Turn);
                StartCoroutine(Rotated(new Vector3(0, -90, 0)));
            }
            if ((Input.GetAxis("Mouse ScrollWheel") > 0) && !_rotating)
            {
                if (Setup.IntToName(Setup.IdCube) == "Wheel1") return;
                delta += 90;
                if (delta == 360) delta = 0;
                PlayAudio(Turn);
                StartCoroutine(Rotated(new Vector3(0, 90, 0)));
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                foreach (Transform child in GameObject.Find("Figures").transform)
                {
                    if (Color.Equals(child.GetComponent<MeshRenderer>().material.color, _red.color))
                        continue;
                    Setup.pokras(child, Setup.MainColor);
                }
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                Testing();
            }
            if (Input.GetKeyDown(KeyCode.V))
            {
                _simMode = !_simMode;
                if (_simMode)
                {
                    _lineObject = Instantiate(_line);
                    _lineObject.name = "Line";
                    _lineObject.transform.eulerAngles = new Vector3(0, 270, 0);
                    _lineObject.transform.position = new Vector3(_lineX, 0.505f, 15f);
                    MadeShablon2();
                    return;
                }
                else
                {
                    Destroy(GameObject.Find("Line"));
                    DeleteShablon2();
                    return;
                }

            }
            if (_simMode)
            {
                var t = _lineObject.transform;
                if (Input.GetKeyDown(KeyCode.LeftBracket))
                {
                    if (_lineX > 0)
                    {
                        _lineX -= 0.5f;
                        var position = t.position;
                        position = new Vector3(_lineX, position.y, position.z);
                        t.position = position;
                        DeleteShablon2();
                        MadeShablon2();
                        return;
                    }
                }
                if (Input.GetKeyDown(KeyCode.RightBracket))
                {
                    if (_lineX < 30)
                    {
                        _lineX += 0.5f;
                        var position = t.position;
                        position = new Vector3(_lineX, position.y, position.z);
                        t.position = position;
                        DeleteShablon2();
                        MadeShablon2();
                        return;
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                if (_hits.Length > 0)
                {
                    foreach (var x in _hits)
                    {
                        var s = x.transform.name;
                        if ((x.transform.CompareTag("Plane")) || (x.transform.name == "Plane")) print(s + " " + x.transform.position + x.transform.parent.name);
                        else print(s + " " + x.transform.position);
                    }
                    print(IndexFigurePlanRay());
                }
            }
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                foreach (Transform child in GameObject.Find("Figures").transform)
                    Destroy(child.gameObject);
                _cpu = 0;
                _health = 0;
                _mass = 0;
                InfoPanelUpdate();
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                if (_hits.Length > 0)
                {
                    foreach (var w in _hits)
                    {
                        if (IsFigure(w.transform.tag))
                        {
                            var t = w.transform;
                            while (IsFigure(t.parent.tag)) t = t.parent.transform;
                            Setup.IdCube = Setup.NameToInt(t.name);
                            PlayAudio(Click);
                            MadeFigureShablon();
                            return;
                        }
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (_hits.Length > 0)
                {
                    if (IsFigure(_hits[0].transform.tag)) return;
                    if (GameObject.FindGameObjectsWithTag("Shablon").Length == 0) return;
                    if (GameObject.Find("Marker").GetComponent<MeshRenderer>().material.color == Color.red)
                        return;
                    PlayAudio(On);
                    var tr = GameObject.FindGameObjectsWithTag("Shablon")[0].transform;
                    DeleteShablon();
                    var xx = Setup.madeFigure(Setup.IdCube, tr.position, tr.eulerAngles, Setup.MainColor);
                    _cpu += Setup.Figures[Setup.IdCube].Cpu;
                    _health += Setup.Figures[Setup.IdCube].Health;
                    _mass += Setup.Figures[Setup.IdCube].Mass;
                    InfoPanelUpdate();
                    RedFigure(xx);
                    if (_simMode)
                    {
                        if (Math.Abs(_lineX - tr.position.x) < 0.2)
                        {
                            return;
                        }
                        if (GameObject.FindGameObjectsWithTag("Shablon2")[0])
                        {
                            if (GameObject.FindGameObjectsWithTag("Shablon2")[0].GetComponent<MeshRenderer>().material.color == _red.color)
                                return;
                        }
                        //добавить проверку на занятую точку
                        if (HaveFigure(new Vector3(2 * (_lineX - tr.position.x) + tr.position.x, tr.position.y, tr.position.z)))
                        {
                            return;
                        }

                        var position = tr.position;
                        var vv = new Vector3(2 * (_lineX - position.x) + position.x, position.y, position.z);
                        var eulerAngles = tr.eulerAngles;
                        var yy = Setup.madeFigure(Setup.IdCube, vv, eulerAngles, Setup.MainColor);
                        _cpu += Setup.Figures[Setup.IdCube].Cpu;
                        _health += Setup.Figures[Setup.IdCube].Health;
                        _mass += Setup.Figures[Setup.IdCube].Mass;
                        InfoPanelUpdate();
                        yy.eulerAngles = SimRotate(eulerAngles);
                        if (yy.name == "Wheel1")
                        {
                            yy.transform.Rotate(0, 180, 0);
                        }
                        RedFigure(yy);
                    }
                }
                if (Setup.Figures[Setup.IdCube].Type == "Weapon")
                {
                    if (!Setup.LA.Contains(Setup.IdCube))
                    {
                        print("если не содержит то добавить");
                        if (Setup.LA[0] == -1) PutOnWeaponPanel(1, Setup.IdCube);
                        else if (Setup.LA[1] == -1) PutOnWeaponPanel(2, Setup.IdCube);
                        else if (Setup.LA[2] == -1) PutOnWeaponPanel(3, Setup.IdCube);

                        TriggerPanel(_weaponPanel);
                        EditButtonsUpdate();
                        GameObject.Find("GridItem" + (_buttonIndex).ToString()).transform.Find("WeaponItem").GetComponent<Button>().Select();
                    }
                }
                return;
            }
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (_hits.Length > 0)
                {
                    foreach (var w in _hits)
                    {
                        if (!IsFigure(w.transform.tag)) continue;
                        var t = w.transform;
                        while (IsFigure(t.parent.tag)) t = t.parent.transform;
                        _cpu -= Setup.Figures[Setup.NameToInt(t.name)].Cpu;
                        _health -= Setup.Figures[Setup.IdCube].Health;
                        _mass -= Setup.Figures[Setup.IdCube].Mass;
                        InfoPanelUpdate();
                        Destroy(t.gameObject);
                        FigToRed(t);
                        MadeFreePlanes(t);
                        if (_simMode && Math.Abs(_lineX - t.position.x) > 0.2f)
                        {
                            var position = t.position;
                            var vv = new Vector3(2 * (_lineX - position.x) + position.x, position.y, position.z);
                            var g = Setup.figureAtPos(vv);
                            //надо добавить проверку что есть объект
                            if (g != null)
                            {
                                _cpu -= Setup.Figures[Setup.NameToInt(g.name)].Cpu;
                                _health -= Setup.Figures[Setup.IdCube].Health;
                                _mass -= Setup.Figures[Setup.IdCube].Mass;
                                InfoPanelUpdate();
                                Destroy(g);
                                FigToRed(g.transform);
                                MadeFreePlanes(g.transform);
                            }
                        }
                        PlayAudio(Off);
                        if (Setup.Figures[Setup.NameToInt(t.name)].Type != "Weapon") return;
                        if (GameObject.Find("Figures").transform.Cast<Transform>().Where(child => child.GetInstanceID() != t.GetInstanceID()).Any(child => t.name == child.name))
                            return;

                        if (Setup.LA.Contains(Setup.IdCube))
                        {
                            PutOnWeaponPanel(Array.IndexOf(Setup.LA, Setup.NameToInt(t.name)) + 1, -1);
                        }
                        return;
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.Mouse4))
            {
                if (SceneManager.GetActiveScene().name == "Edit") Setup.SaveBlocks();
#if UNITY_EDITOR
                Setup.Figure1.color = new Color32(58, 90, 186, 255);
                UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
            }
            //луч никуда не попал
            if (_hits.Length == 0)
            {
                DeleteShablon();
                return;
            }

            if (_hits.Length <= 0) return;
            {
                var index = -1;
                if (HaveFigureRay())
                {
                    index = IndexFigurePlanRay();
                    if (index >= 0)
                    {
                        //print ("есть фигура c плоскостью index="+index);
                    }
                    else
                    {
                        //print ("есть фигура без плоскости");
                        DeleteShablon();
                        //Setup.pokras (GameObject.Find("FigureShablon").transform, Setup.MainColor);
                        RefreshFigureShablonColor(Setup.MainColor);
                        return;
                    }
                }
                else
                {
                    if (HavePlanPRay())
                    {
                        index = IndexPlanPRay();
                        //print ("есть пол index="+index);
                    }
                    else
                    {
                        //print ("нет фигуры и пола");
                        DeleteShablon();
                        //Setup.pokras (GameObject.Find("FigureShablon").transform, Setup.MainColor);
                        RefreshFigureShablonColor(Setup.MainColor);
                        return;
                    }
                }

                // вычисляем позицию для фигуры
                var ray = _hits[index];
                //print (Ray.transform.position);
                var pos = PosFigureOnPlane(ray.transform);
                // если позиця уже занята то выходим
                /*if(haveFigurePlane (pos, Ray.transform.rotation)) {
				DeleteShablon();
				RefreshFigureShablonColor (Setup.MainColor);
				return;
			}*/

                if (HaveFigure(pos))
                {
                    DeleteShablon();
                    RefreshFigureShablonColor(Setup.MainColor);
                    //print (4);
                    return;
                }

                // колеса только слева и справа
                if (
                    (Setup.IntToName(Setup.IdCube) == "Wheel") ||
                    (Setup.IntToName(Setup.IdCube) == "Wheel1")
                )
                {
                    var s = Setup.naklon(ray.transform.eulerAngles);

                    if (s != "left" && s != "right")
                    {
                        DeleteShablon();
                        RefreshFigureShablonColor(Setup.MainColor);
                        return;
                    }
                }

                //ПРОВЕРКА
                if (GameObject.FindGameObjectsWithTag("Shablon").Length > 0)
                {
                    var ii = GameObject.FindGameObjectsWithTag("Shablon")[0];
                    if (
                        (ii.transform.position == pos) &&
                        ((ii.name == "Wheel1") ||
                         (ii.transform.rotation == ray.transform.rotation * Quaternion.Euler(0, delta, 0))) &&
                        (ii.name == Setup.IntToName(Setup.IdCube)) &&
                        (GameObject.Find("FigureShablon").transform.GetComponent<MeshRenderer>().materials[0].color ==
                         Setup.MainColor)
                    )
                    {
                        return;
                    }
                }

                // удаляем шаблон нового кубика
                DeleteShablon();
                // создаем шаблон нового кубика
                //Setup.pokras (GameObject.Find("FigureShablon").transform, Setup.MainColor);
                RefreshFigureShablonColor(Setup.MainColor);


                _nObject = MadeShablon("Shablon", pos, ray.transform.rotation, true);
                _nObject.AddComponent<Shablon>();
                //nObject.GetComponent<MeshCollider> ().isTrigger = true;
                _nObject.AddComponent<Rigidbody>();
                _nObject.GetComponent<Rigidbody>().isKinematic = true;
                _nObject.GetComponent<Rigidbody>().useGravity = false;

                // создаем подсветку примыкающей плоскости
                _pMarker = Instantiate(_marker, ray.transform.position, _nObject.transform.rotation);
                _pMarker.transform.parent = _nObject.transform;
                _pMarker.transform.GetComponent<MeshRenderer>().material = _shablon;
                _pMarker.transform.localPosition = new Vector3(0, 0.02f, 0);
                _pMarker.name = "Marker";
                if (_simMode)
                {
                    MadeShablon2();
                }
            }
        }

        private void MadeShablon2()
        {
            if (_nObject == null)
                return;
            var nX = _nObject.transform.position.x;
            if ((2 * (_lineX - nX) + nX) < 0)
                return;
            if ((2 * (_lineX - nX) + nX) > 30)
                return;
            if (Math.Abs(_lineX - nX) < 0.2)
            {
                return;
            }
            if (HaveFigure(new Vector3(2 * (_lineX - nX)
                                       + nX, _nObject.transform.position.y, _nObject.transform.position.z)))
            {
                return;
            }

            var position = _nObject.transform.position;
            var vv = new Vector3(2 * (_lineX - nX) +
                                 nX, position.y, position.z);
            var shablon2 = MadeShablon("Shablon2", vv, _nObject.transform.rotation);
            shablon2.transform.eulerAngles = SimRotate(_nObject.transform.eulerAngles);

            if (shablon2.name == "Wheel1")
            {
                shablon2.transform.Rotate(0, 180, 0);
            }
            shablon2.AddComponent<Shablon>();
            shablon2.AddComponent<Rigidbody>();
            shablon2.GetComponent<Rigidbody>().isKinematic = true;
            shablon2.GetComponent<Rigidbody>().useGravity = false;
        }

        private bool HaveFigureRay()
        {
            return _hits.Any(r => r.transform.CompareTag("Figure"));
        }

        private int IndexFigure()
        {
            var i = 0;
            foreach (var r in _hits)
            {
                if (r.transform.CompareTag("Figure"))
                {
                    return i;
                }
                i++;
            }
            return -1;
        }

        private int IndexFigurePlanRay()
        {
            var i = 0;
            foreach (var r in _hits)
            {
                if (r.transform.CompareTag("Figure"))
                {
                    if (i == 0)
                        return -1;
                    if (_hits[i - 1].transform.CompareTag("Plane"))
                        return i - 1;
                    else
                        return -1;
                }
                i++;
            }
            return -1;
        }

        private bool HavePlanPRay()
        {
            return _hits.Any(r => r.transform.name == "PlaneP");
        }

        private int IndexPlanPRay()
        {
            var i = 0;
            foreach (var r in _hits)
            {
                if (r.transform.name == "PlaneP")
                {
                    return i;
                }
                i++;
            }
            return -1;
        }

        public void OnGUI()
        {
            if (!_figurePanel.activeSelf && !Setup.Pause)
                GUI.DrawTexture(new Rect(Screen.width / 2 - 5, Screen.height / 2 - 5, 10, 10), pricel);
        }
        void DeleteShablon()
        {
            GameObject[] s1 = GameObject.FindGameObjectsWithTag("Shablon");
            if (s1.Length > 0) Destroy(s1[0]);
            GameObject[] s2 = GameObject.FindGameObjectsWithTag("Shablon2");
            if (s2.Length > 0) Destroy(s2[0]);
        }
        void DeleteShablon2()
        {
            GameObject[] s2 = GameObject.FindGameObjectsWithTag("Shablon2");
            if (s2.Length > 0) Destroy(s2[0]);
        }

        private GameObject MadeShablon(string tag, Vector3 pos, Quaternion rot, bool withDelta = false)
        {
            var newFigure = GameObject.Instantiate(Setup.Figures[Setup.IdCube].GO, pos, rot);
            newFigure.transform.localScale = new Vector3(0.499f, 0.499f, 0.499f);
            newFigure.name = Setup.Figures[Setup.IdCube].Name;
            var ns = newFigure.name;
            if ((ns != "Wheel1") && withDelta)
            {
                newFigure.transform.rotation *= Quaternion.Euler(0, delta, 0);
            }
            SettingsMesh(newFigure.transform);
            foreach (Transform child in newFigure.transform)
            {
                if (child.CompareTag("Plane"))
                {
                    Destroy(child.gameObject);
                }
            }
            Setup.Tagged(newFigure.transform, tag);
            Сlarify(newFigure);
            return newFigure;
        }

        private static void SettingsMesh(Transform x)
        {
            if (!x.gameObject.GetComponent<MeshCollider>())
            {
                x.gameObject.AddComponent<MeshCollider>();
            }
            x.gameObject.GetComponent<MeshCollider>().convex = true;
            x.gameObject.GetComponent<MeshCollider>().isTrigger = true;
            foreach (Transform child in x)
            {
                if (!child.CompareTag("Plane"))
                    SettingsMesh(child);
            }
        }

        private static Color Transparent(Color x)
        {
            x.a = 0.5f;
            return x;
        }

        private void Сlarify(GameObject g)
        {
            var l = g.transform.GetComponent<MeshRenderer>().materials.Length;
            var ca = new Color[l];
            var xMat = new Material[l];
            xMat[0] = _shablon;
            ca[0] = Transparent(Setup.MainColor);
            for (int x = 1; x < l; x++)
            {
                xMat[x] = _shablon;
                ca[x] = Transparent(g.transform.GetComponent<MeshRenderer>().materials[x].color);
            }
            g.transform.GetComponent<MeshRenderer>().materials = xMat;
            for (int x = 0; x < l; x++)
            {
                g.transform.GetComponent<MeshRenderer>().materials[x].color = ca[x];
            }
            foreach (Transform child in g.transform)
            {
                if (!child.CompareTag("Plane"))
                    Сlarify(child.gameObject);
            }
        }

        private Vector3 SimRotate(Vector3 f)
        {
            var s = Setup.naklon(f);
            var sa = Setup.Figures[Setup.IdCube].Sim;
            switch (s)
            {
                case "top":
                case "down":
                {
                    switch (sa)
                    {
                        case 180:
                        {
                            if (delta == 0 || delta == 180)
                            {
                                f += new Vector3(0, 180, 0);
                            }

                            break;
                        }
                        case -90 when delta == 0 || delta == 180:
                            f += new Vector3(0, 0, 0);
                            break;
                        case -90:
                            f += new Vector3(0, 180, 0);
                            break;
                        case 90:
                        {
                            switch (s)
                            {
                                case "top" when delta == 0 || delta == 180:
                                    f += new Vector3(0, 90, 0);
                                    break;
                                case "top":
                                case "down" when delta == 0 || delta == 180:
                                    f += new Vector3(0, -90, 0);
                                    break;
                                case "down":
                                    f += new Vector3(0, 90, 0);
                                    break;
                            }

                            break;
                        }
                    }

                    break;
                }
                case "forward":
                case "back":
                {
                    switch (sa)
                    {
                        case 180:
                        {
                            if (delta == 0 || delta == 180)
                            {
                                f += new Vector3(0, 180, 180);
                            }

                            break;
                        }
                        case -90 when delta == 0 || delta == 180:
                            f += new Vector3(0, 0, 0);
                            break;
                        case -90:
                            f += new Vector3(180, 0, 180);
                            break;
                        case 90:
                        {
                            switch (delta)
                            {
                                case 0:
                                    f += new Vector3(-90, 90, -90);
                                    break;
                                case 90:
                                    f += new Vector3(90, -90, 90);
                                    break;
                                case 180:
                                    f += new Vector3(90, -90, -90);
                                    break;
                                case 270:
                                    f += new Vector3(-90, -90, -90);
                                    break;
                            }

                            break;
                        }
                    }

                    break;
                }
                case "left":
                case "right":
                {
                    switch (sa)
                    {
                        case 180:
                            f += new Vector3(0, 0, 180);
                            break;
                        case -90 when delta == 90 || delta == 270:
                            f += new Vector3(180, 180, 180);
                            break;
                        case -90:
                            f += new Vector3(180, 0, 180);
                            break;
                        case 90:
                        {
                            switch (delta)
                            {
                                case 0:
                                    f += new Vector3(-90, 180, 0);
                                    break;
                                case 90:
                                    f += new Vector3(90, 90, 90);
                                    break;
                                case 180:
                                    f += new Vector3(90, 180, 0);
                                    break;
                                case 270:
                                    f += new Vector3(90, -90, 90);
                                    break;
                            }

                            break;
                        }
                    }

                    break;
                }
            }

            return f;
        }

        private static bool HaveFigure(Vector3 x)
        {
            for (int i = 0; i < GameObject.Find("Figures").transform.childCount; i++)
            {
                var c = GameObject.Find("Figures").transform.GetChild(i);
                if (Vector3.Distance(x, c.position) < 0.02f)
                {
                    //print (c.position+" "+x);
                    return true;
                }
            }
            return false;
        }

        private bool HaveFigurePlane(Vector3 pos, Quaternion rot)
        {
            if (Setup.Figures[Setup.IdCube].GO.name != "Wheel1")
            {
                rot *= Quaternion.Euler(0, delta, 0);
            }
            if (GameObject.Find("Empty") != null) Destroy(GameObject.Find("Empty"));
            var empty = GameObject.Instantiate(Setup.Figures[Setup.IdCube].GO, pos, rot);
            empty.tag = "Untagged";

            empty.transform.localScale = new Vector3(0.49f, 0.49f, 0.49f);
            /*
		if (empty.name == "Rotor1") {
			empty.transform.localScale = new Vector3 (0.49f, 0.2f, 0.49f);
			empty.transform.localPosition-=new Vector3(0,0.02f,0);
		}
		*/
            empty.name = "Empty";
            empty.GetComponent<MeshRenderer>().enabled = false;
            empty.GetComponent<MeshCollider>().enabled = false;
            foreach (Transform child in empty.transform)
            {
                if (child.CompareTag("Plane"))
                {
                    child.transform.localScale = new Vector3(0.190f, 0.0001f, 0.190f);
                    child.name = "P";
                    child.tag = "Untagged";
                    child.GetComponent<MeshCollider>().enabled = false;
                }
                else
                    Destroy(child.gameObject);
            }
            foreach (Transform child in empty.transform)
            {
                foreach (var plane in GameObject.FindGameObjectsWithTag("Plane"))
                {
                    if (!(Vector3.Distance(child.position, plane.transform.position) < 0.0099f)) continue;
                    //plane.GetComponent<MeshRenderer> ().enabled = true;
                    var r = child.position - plane.transform.position;
                    print(r.x + " " + r.y + " " + r.z);
                    //Destroy(GameObject.Find("Empty"));
                    return true;
                }
            }

            //Destroy(GameObject.Find("Empty"));
            return false;
        }

        private bool RedFigure(Transform x)
        {
            if (GameObject.Find("Figures").transform.childCount == 1)
                return false;
            //какие рядом фигуры?
            var figs = 0;
            var reds = 0;
            foreach (Transform child in x)
            {
                if (!child.CompareTag("Plane")) continue;
                var g = Setup.figureAtPlane(child);
                if (g == null) continue;
                if (Color.Equals(_red.color, g.transform.GetComponent<MeshRenderer>().material.color))
                {
                    reds++;
                }
                else
                    figs++;
            }
            //print ("figs="+figs+" reds="+reds);
            //если рядом обычных нет то красим в красный и выходим
            if (figs == 0)
            {
                RedPokras(x);
                return false;
            }
            //если рядом обычные и красных нет то выходим
            if (reds == 0)
            {
                return true;
            }
            //если рядом обычные и красные
            RedToFig(x);
            return true;
        }

        private bool RedToFig(Transform x)
        {
            foreach (Transform child in x)
            {
                if (child.CompareTag("Plane"))
                {
                    GameObject g = Setup.figureAtPlane(child);
                    if (g != null)
                    {
                        if (Color.Equals(_red.color, g.transform.GetComponent<MeshRenderer>().material.color))
                        {
                            AntiRedPokras(g.transform);////красим кубик из красного
                            RedToFig(g.transform);
                        }
                    }
                }
            }
            return true;
        }

        private bool FigToRed(Transform x)
        {
            if (Color.Equals(_red.color, x.GetComponent<MeshRenderer>().material.color))
            {
                return true;
            }
            int joinFigureCount = JoinFiguresCount(x);
            if (joinFigureCount == 1) return true;
            int[] rr = new int[joinFigureCount];
            Transform[] rf = new Transform[joinFigureCount];
            int i = 0;
            x.tag = "checkFigure";
            foreach (Transform child in x)
            {
                if (child.name == "Join")
                {
                    rr[i] = 0;
                    GameObject g = Setup.figureAtPlane(child);
                    if ((g != null) && (!g.CompareTag("checkFigure")))
                    {
                        rf[i] = g.transform;
                        g.tag = "checkFigure";
                        rr[i]++;
                        foreach (Transform child2 in g.transform)
                        {
                            if (child2.name != "Join") continue;
                            if (!(Vector3.Distance(child.position, child2.position) < 0.02f)) continue;
                            child.name = "Plane";//
                            child2.name = "Plane";//
                            rr[i] += FigToRed2(g.transform);
                        }
                        i++;
                    }
                }
            }
            //Красим все группы кубиков в красный кроме самой большой группы
            if (joinFigureCount > 1)
            {
                var max = rr.Max();
                for (int y = 0; y < joinFigureCount; y++)
                {
                    if (max == rr[y]) continue;
                    if (rr[y] > 0) JustRedFigures(rf[y]);
                }
            }
            foreach (GameObject child in GameObject.FindGameObjectsWithTag("checkFigure"))
            {
                child.tag = "Figure";
            }
            return true;
        }

        private static int FigToRed2(Transform x)
        {
            var count = 0;
            foreach (Transform child in x)
            {
                if (child.name != "Join") continue;
                GameObject g = Setup.figureAtPlane(child);
                if ((g != null) && (!g.CompareTag("checkFigure")))
                {
                    g.tag = "checkFigure";
                    count++;
                    foreach (Transform child2 in g.transform)
                    {
                        if (child2.name == "Join")
                        {
                            if (Vector3.Distance(child.position, child2.position) < 0.02f)
                            {
                                count += FigToRed2(g.transform);
                            }
                        }
                    }
                }
            }
            return count;
        }

        private static bool IsFigure(string s)
        {
            return s == "Figure";
        }

        private static int JoinFiguresCount(Transform t)
        {
            return t.Cast<Transform>().Count(child => child.name == "Join");
        }

        private static Vector3 PosFigureOnPlane(Transform x)
        {
            var p = x.position;
            if (x.name == "PlaneP")
            {
                return new Vector3(p.x, p.y + 1, p.z);
            }
            var s = Setup.naklon(x.eulerAngles);
            switch (s)
            {
                case "top":
                    return new Vector3(p.x, p.y + 0.5f, p.z);
                case "down":
                    return new Vector3(p.x, p.y - 0.5f, p.z);
                case "left":
                    return new Vector3(p.x - 0.5f, p.y, p.z);
                case "right":
                    return new Vector3(p.x + 0.5f, p.y, p.z);
                case "back":
                    return new Vector3(p.x, p.y, p.z - 0.5f);
                case "forward":
                    return new Vector3(p.x, p.y, p.z + 0.5f);
                default:
                    return new Vector3(p.x, p.y, p.z);
            }
        }

        private void MadeJoinedPlanes(Transform x)
        {
            foreach (Transform child in x)
            {
                if (!child.CompareTag("Plane")) continue;
                var g = Setup.figureAtPlane(child);
                if (g != null)
                {
                    foreach (Transform child2 in g.transform)
                    {
                        if (!child2.CompareTag("Plane")) continue;
                        if (!(Vector3.Distance(child.position, child2.position) < 0.02f)) continue;
                        child.name = "Join";
                        child2.name = "Join";
                    }
                }
                else
                {
                    child.name = "Plane";
                }
            }
        }

        private static void MadeFreePlanes(Transform x)
        {
            foreach (Transform child in x)
            {
                if (child.name != "Join") continue;
                var g = Setup.figureAtPlane(child);
                if (g == null) continue;
                foreach (Transform child2 in g.transform)
                {
                    if (child2.name != "Join") continue;
                    if (Vector3.Distance(child.position, child2.position) < 0.02f)
                    {
                        child2.name = "Plane";
                    }
                }
            }
        }

        private bool JustRedFigures(Transform x)
        {
            if (Color.Equals(_red.color, x.GetComponent<MeshRenderer>().material.color))
            {
                return true;
            }
            RedPokras(x);
            foreach (Transform child in x)
            {
                if (child.name == "Join")
                {
                    GameObject g = Setup.figureAtPlane(child);
                    JustRedFigures(g.transform);
                }
            }
            return true;
        }

        private void RedPokras(Transform x)
        {
            if (x.parent.name == "Figures")
            {
                _colorArr.Add(x.gameObject.GetInstanceID(), x.GetComponent<MeshRenderer>().material.color);
            }
            int l = x.GetComponent<MeshRenderer>().materials.Length;
            Material[] xMat = new Material[l];
            for (int i = 0; i < l; i++)
            {
                xMat[i] = _red;
            }
            x.GetComponent<MeshRenderer>().materials = xMat;
            foreach (Transform child in x)
            {
                if (!child.CompareTag("Plane"))
                {
                    RedPokras(child);
                }

            }
        }

        private void AntiRedPokras(Transform x)
        {
            var col = _colorArr[x.gameObject.GetInstanceID()];
            _colorArr.Remove(x.gameObject.GetInstanceID());
            var t = Setup.Figures[Setup.NameToInt(x.name)].GO.transform;
            x.GetComponent<MeshRenderer>().sharedMaterials = t.GetComponent<MeshRenderer>().sharedMaterials;
            x.GetComponent<MeshRenderer>().material.color = col;
            foreach (Transform child in x)
            {
                if (child.CompareTag("Plane")) continue;
                child.GetComponent<MeshRenderer>().sharedMaterials =
                    t.Find(child.name).GetComponent<MeshRenderer>().sharedMaterials;
                child.GetComponent<MeshRenderer>().material.color = col;
                var t2 = t.Find(child.name);
                foreach (Transform child2 in child)
                {
                    if (child2.CompareTag("Plane")) continue;
                    child2.GetComponent<MeshRenderer>().sharedMaterials =
                        t2.Find(child2.name).GetComponent<MeshRenderer>().sharedMaterials;
                    child2.GetComponent<MeshRenderer>().material.color = col;
                }

            }
        }

        private void Testing()
        {
            foreach (var child in _hits)
            {
                print(child.transform.name);
            }

            if (_hits.Length <= 0) return;
            var index = -1;
            if (!HaveFigureRay()) return;
            index = IndexFigurePlanRay();
            if (index >= 0)
            {
                print(_hits[index + 1].transform.name + " " + GetMeshIndex(_hits[index + 1]));
            }
            else
            {
                index = IndexFigure();
                print(_hits[index].transform.name + " " + GetMeshIndex(_hits[index]));
            }
        }

        private static int GetMeshIndex(RaycastHit r)
        {
            var mf = (MeshFilter)r.transform.gameObject.GetComponent(typeof(MeshFilter));
            var mesh = mf.mesh;

            var totalSubMeshes = mesh.subMeshCount;
            var subMeshesFaceTotals = new int[totalSubMeshes];

            for (int i = 0; i < totalSubMeshes; i++)
            {
                subMeshesFaceTotals[i] = mesh.GetTriangles(i).Length / 3;
            }
            var hitSubMeshNumber = 0;
            var maxVal = 0;

            for (int i = 0; i < totalSubMeshes; i++)
            {
                maxVal += subMeshesFaceTotals[i];

                if (r.triangleIndex > maxVal - 1) continue;
                hitSubMeshNumber = i + 1;
                break;

            }

            Debug.Log("We hit sub mesh number: " + hitSubMeshNumber);
            return hitSubMeshNumber;
        }
    }
}
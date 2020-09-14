using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using static RuntimePreviewGenerator;
public static class Setup
{
	public static int WheelsCount;
	public static GameObject[] wo;
	public static GameObject[] co;
	public static Material innerFadeMaterial;
	public static Material outerFadeMaterial;
    public const int FiguresCount = 20;
	public static Figure[] Figures = new Figure[FiguresCount];
    public static bool music = true;
	public static Color MainColor = Color.black;
    public static bool Pause=false;
    public static int IdActiveRobot=0;
    public static int RobotsCount=2;
    public static int IdCube = 0;
	public static int IdOldCube = -1;
    public static Material Figure1;
	public static Boolean shab;
	static string[] StrPalette = new string[32];
	public static Color[] Palette = new Color[32];
	public static int[] LA = new int[3];
	public static GameObject EscapePanel;
    public static void Start()
    {
		GameObject enemy = Resources.Load("Models/EscapePanel", typeof(GameObject)) as GameObject;
		EscapePanel = GameObject.Instantiate(enemy/*, Vector3.zero, Quaternion.identity,*/, GameObject.Find("Canvas").transform);
		EscapePanel.name="EscapePanel";
		EscapePanel.SetActive (true);
		GameObject.Find ("Canvas").AddComponent<EscapePanel> ();
		//PlayerPrefs.SetInt("RobotsCount", 1);
		if (PlayerPrefs.HasKey("RobotsCount")) RobotsCount = PlayerPrefs.GetInt("RobotsCount");
		if (PlayerPrefs.HasKey("IdActiveRobot")) IdActiveRobot = PlayerPrefs.GetInt("IdActiveRobot");

		innerFadeMaterial = Resources.Load("Materials/LaserLine", typeof(Material)) as Material;
		outerFadeMaterial = Resources.Load("Materials/LaserLine", typeof(Material)) as Material;
		Figure1 = Resources.Load("Materials/Figure1", typeof(Material)) as Material;

		Figures = new Figure[FiguresCount]{
			new Figure ("Cubes", "CubeBase6Faces", name : "Cube"),
			new Figure ("Cubes", "CubeBase5Faces", 180, name : "Cube"),
			new Figure ("Cubes", "CubeBase2Faces", 180, name : "Cube"),
			new Figure ("Cubes", "Pokat4",180, name : "Edge"),
			new Figure ("Cubes", "VeryPokat",180, name : "Edge Round"),
			new Figure ("Cubes", "VeryVognut",180, name : "Edge Slope"),
			new Figure ("Cubes", "ugol", 90, name : "Corner"),
			new Figure ("Cubes", "piramid", 90, name : "Piramid"),
			new Figure ("Cubes", "PokatPiramid", 90, name : "Cone"),
			new Figure ("Cubes", "neugol", 90, name : "Inner"),
			new Figure ("Cubes", "VeryPokat3Ugol", 90, name : "Inner Round"),
			new Figure ("Rotors", "Rotor1", 180, name : "T0 Rotor", cpu:10),
			new Figure ("Lasers", "Laser1", 180, name : "T0 Laser", cpu:10,type:"Weapon"),
			new Figure ("Lasers", "Laser1-static", 180, name : "T0 Laser Static", cpu:10,type:"Weapon"),
			new Figure ("Struts", "Plast", -90, name : "Strut Short", cpu:6),
			new Figure ("Struts", "Plast3x3", name : "Strut Plus", cpu:9),
			new Figure ("Wheels", "Wheel1", 180, name : "Wheel1", cpu:10),
			new Figure ("Rods", "Rod2d1", 180, name : "Rod Short", cpu:2),
			new Figure ("Rods", "Rod2d2", 180, name : "Rod Short 2D", cpu:3),
			new Figure ("Rods", "Rod2d3", 90, name : "Rod Short 3D", cpu:4),
		};
		StrPalette = new string[]{
			"d6a090","fe3b1e","a12c32","fa2f7a","fb9fda","e61cf7","992f7c","47011f",
			"051155","4f02ec","2d69cb","00a6ee","6febff","08a29a","2a666a","063619",
			"000000","4a4957","8e7ba4","b7c0ff","ffffff","acbe9c","827c70","5a3b1c",
			"ae6507","f7aa30","f4ea5c","9b9500","566204","11963b","51e113","08fdcc"
		};
		for (int i = 0; i < 32; i++) {
			ColorUtility.TryParseHtmlString ("#" + StrPalette [i], out Palette [i]);
		}

    }
    
	public static void LoadBlocks(ref int counter)
    {
		counter = 0;
        //если нету записи, то выходим
        if (!PlayerPrefs.HasKey(string.Format("robot{0}", Setup.IdActiveRobot)))
            return;
        //получаем строку, содержащую все блоки
        string strBlocks = PlayerPrefs.GetString(string.Format("robot{0}", Setup.IdActiveRobot));
        //получаем массив блоков
        string[] masBlocks = strBlocks.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
        //перебираем каждый блок
        foreach (string s in masBlocks)
        {
            //Массив параметров блока
            string[] masParam = s.Split(new char[] { '#' }, System.StringSplitOptions.RemoveEmptyEntries);
            //Позиция
            Vector3 pos = new Vector3(float.Parse(masParam[0]), float.Parse(masParam[1]), float.Parse(masParam[2]));
            //цвет
            Color color = IntToColor(int.Parse(masParam[3]));
            //номер
            int id = int.Parse(masParam[4]);
            //наклон
            Vector3 rot;
            rot.x = float.Parse(masParam[5]);
            rot.y = float.Parse(masParam[6]);
            rot.z = float.Parse(masParam[7]);
            //Добавляем блок на сцену
            madeFigure(id, pos, rot, color);
			counter += Setup.Figures [id].Cpu;
        }
		string L = PlayerPrefs.GetString(string.Format("LA{0}", Setup.IdActiveRobot));
		if (!PlayerPrefs.HasKey(string.Format("LA{0}", Setup.IdActiveRobot)))L="-1,-1,-1";
		Setup.LA = L.Split(',').Select(Int32.Parse).ToArray();
    }
    public static IEnumerator LoadRobot(String strBlocks)
    {  
        //получаем массив блоков
        string[] masBlocks = strBlocks.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
        //перебираем каждый блок
		Transform papa=GameObject.Find ("Figures").transform;
        foreach (string s in masBlocks)
        {
            //Массив параметров блока
            string[] masParam = s.Split(new char[] { '#' }, System.StringSplitOptions.RemoveEmptyEntries);
            //Позиция
            Vector3 pos = new Vector3(float.Parse(masParam[0]), float.Parse(masParam[1]), float.Parse(masParam[2]));
            //цвет
            Color color = IntToColor(int.Parse(masParam[3]));
            //номер
            int id = int.Parse(masParam[4]);
            //наклон
            Vector3 rot;
            rot.x = float.Parse(masParam[5]);
            rot.y = float.Parse(masParam[6]);
            rot.z = float.Parse(masParam[7]);
            //Добавляем блок на сцену
			AddFigure (id, pos, rot, color, papa, "Figure");
            yield return null;
        }
        //Debug.Log(string.Format("Загружено {0} блоков!", masBlocks.Length));
    }
    public static void SaveBlocks()
    {
        //получаем число блоков на сцене
        int i = GameObject.Find("Figures").transform.childCount;
        string saveStr = "";
        //берем каждый блок
        for (int x = 0; x < i; x++)
        //foreach (GameObject g in allBlocks)
        {
            GameObject g = GameObject.Find("Figures").transform.GetChild(x).gameObject;
            //позиция блока
            Vector3 pos = g.transform.position;
            //Поворот блока
            Vector3 rot = g.transform.localEulerAngles;
            //Цвет блока
            Color color = g.GetComponent<MeshRenderer>().material.color;
            //Тип блока
            int id = Setup.NameToInt(g.name);
            if (id == -1)
                continue;
            //записываем информацию о блоке в строку.
            saveStr += string.Format("{0}#{1}#{2}#{3}#{4}#{5}#{6}#{7};", fround(pos.x), fround(pos.y), fround(pos.z), Setup.ColorToInt(color), id, rot.x, rot.y, rot.z);
        }
        //добавляем строку с блоками в хронилище
        PlayerPrefs.SetString(string.Format("robot{0}",Setup.IdActiveRobot), saveStr);
        PlayerPrefs.Save();
        //Debug.Log(string.Format("Сохранено {0} блоков", i));
        //Сохроняем превью
        Texture2D t = new Texture2D(200, 170);
		RuntimePreviewGenerator.BackgroundColor = new Color(34f/255,39f/255,48f/255,1f);
        RuntimePreviewGenerator.PreviewDirection = new Vector3(8, -8, -8);
        RuntimePreviewGenerator.OrthographicMode = true;
		t = (Texture2D)RuntimePreviewGenerator.GenerateModelPreview(GameObject.Find("Figures").transform, 200, 170, false);
        Setup.WriteTextureToPlayerPrefs(string.Format("preview{0}", Setup.IdActiveRobot), t);

		string L= string.Join(",",LA);
		//Debug.Log(L);
		PlayerPrefs.SetString(string.Format("LA{0}",Setup.IdActiveRobot),L);

    }
    public static void WriteTextureToPlayerPrefs(string tag, Texture2D tex)
    {
        // if texture is png otherwise you can use tex.EncodeToJPG().
		if (tex == null) {
			PlayerPrefs.SetString(tag, null);
			PlayerPrefs.Save();
			return;
		}
        tex = duplicateTexture(tex);
        byte[] texByte = tex.EncodeToPNG();

        // convert byte array to base64 string
        string base64Tex = System.Convert.ToBase64String(texByte);

        // write string to playerpref
        PlayerPrefs.SetString(tag, base64Tex);
        PlayerPrefs.Save();
    }

    public static Texture2D ReadTextureFromPlayerPrefs(string tag)
    {
        // load string from playerpref
        string base64Tex = PlayerPrefs.GetString(tag, null);

        if (!string.IsNullOrEmpty(base64Tex))
        {
            // convert it to byte array
            byte[] texByte = System.Convert.FromBase64String(base64Tex);
            Texture2D tex = new Texture2D(2, 2);

            //load texture from byte array
            if (tex.LoadImage(texByte))
            {
                return tex;
            }
        }

        return null;
    }
    public static Texture2D duplicateTexture(Texture2D source)
    {
		if (source == null)
			return null;
        RenderTexture renderTex = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);
        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }
    public static Transform madeFigure(int i, Vector3 pos, Vector3 angles, Color svet)
    {
		GameObject newFigure = GameObject.Instantiate(Figures[i].GO);
		newFigure.transform.localScale=new Vector3(0.4999f,0.4999f,0.4999f);
        newFigure.transform.position = new Vector3(fround(pos.x), fround(pos.y), fround(pos.z));
        newFigure.transform.eulerAngles = new Vector3(Mathf.Round(angles.x), Mathf.Round(angles.y), Mathf.Round(angles.z));
        newFigure.name = Setup.IntToName(i);
		if (!newFigure.GetComponent<MeshCollider> ()) {
			newFigure.AddComponent<MeshCollider>();
		}
		newFigure.gameObject.GetComponent<MeshCollider>().convex = true;
        newFigure.transform.parent = GameObject.Find("Figures").transform;
        foreach (Transform child in newFigure.transform)
        {
			if (child.tag != "Plane") {
				if (!child.GetComponent<MeshCollider> ()) {
					child.gameObject.AddComponent<MeshCollider> ();
				}
			}
        }
		pokras (newFigure.transform, svet);
		Tagged(newFigure.transform,"Figure");
        foreach (Transform child in newFigure.transform) {
			if (child.tag == "Plane") {
				child.gameObject.GetComponent<MeshCollider> ().convex = true;
				child.eulerAngles = norm (child);
				child.transform.localScale = new Vector3 (0.190f, 0.0001f, 0.190f);


			} 
        }
        madeJoinedPlanes(newFigure.transform);
        return newFigure.transform;
    }

	public static void AddFigure(int id, Vector3 pos, Vector3 angles, Color color,Transform papa,string tag)
	{
		GameObject newFigure = GameObject.Instantiate(Figures[id].GO);
		newFigure.transform.localScale=new Vector3(0.5f,0.5f,0.5f);
		newFigure.transform.position = new Vector3(Setup.fround(pos.x), Setup.fround(pos.y), Setup.fround(pos.z)); ;
		newFigure.transform.eulerAngles = new Vector3(Mathf.Round(angles.x), Mathf.Round(angles.y), Mathf.Round(angles.z));
		newFigure.name = Setup.IntToName(id);
		newFigure.transform.parent = papa;
		foreach (Transform child in newFigure.transform) {
			if (child.tag == "Plane") {
				GameObject.Destroy (child.gameObject);
			}
		}
		Setup.pokras(newFigure.transform,color);
		Setup.Tagged (newFigure.transform, tag);
	}

	public static GameObject AddFigureWithPlanes(int id, Vector3 pos, Vector3 angles, Color color,Transform papa,string tag)
	{
		GameObject newFigure = GameObject.Instantiate(Figures[id].GO);
		newFigure.transform.localScale=new Vector3(0.5f,0.5f,0.5f);
		newFigure.transform.position = new Vector3(Setup.fround(pos.x), Setup.fround(pos.y), Setup.fround(pos.z)); ;
		newFigure.transform.eulerAngles = new Vector3(Mathf.Round(angles.x), Mathf.Round(angles.y), Mathf.Round(angles.z));
		newFigure.name = Setup.IntToName(id);
		newFigure.transform.parent = papa;
		foreach (Transform child in newFigure.transform) {
			if (child.tag == "Plane") {
				child.eulerAngles = norm (child);
				child.transform.localScale = new Vector3 (0.19f, 0.0001f, 0.19f);
			}
		}
		Setup.pokras(newFigure.transform,color);
		Setup.Tagged (newFigure.transform, tag);
		Setup.madeJoinedPlanes2(newFigure.transform);
		return newFigure;
	}

	public static void pokras(Transform x,Color c)
	{
		x.GetComponent<MeshRenderer>().materials[0] = Figure1;
		x.GetComponent<MeshRenderer>().materials[0].color = c;
		foreach (Transform child in x) {
			if (child.tag != "Plane") {
				pokras (child, c);
			}
		}
	}
	public static void Tagged(Transform x,String tag)
	{
		x.tag = tag;
		foreach (Transform child in x) {
			if(child.tag!="Plane")
			Tagged (child, tag);
		}
	}

	public static void Layered(GameObject x,int Layer)
	{
		x.layer = Layer;
		foreach (Transform child in x.transform) {
			Layered (child.gameObject,Layer);
		}
	}

    public static string naklon(Vector3 v)
    {
        float x = Mathf.Round(v.x);
        float y = Mathf.Round(v.y);
        float z = Mathf.Round(v.z);
        if ((x == 0) && (z == 0))
        {
            return "top";
        }
        else
        {
            if ((x == 0) && (z == 180))
            {
                return "down";
            }
            if (z != 180)
            {
                if (((x == 0) && (y + z == 360)) || ((z == 0) && (x + y == 270)))
                {
                    return "back";
                }
                if (((x == 0) && (y == z) && (y > 0)) || ((z == 0) && (x - y == 90)))
                {
                    return "forward";
                }
                if (((x == 0) && (y + z == 270)) || ((z == 0) && (x == y) && (x > 0)))
                {
                    return "right";
                }
                if (((x == 0) && (z - y == 90)) || ((z == 0) && (x + y == 360)))
                {
                    return "left";
                }
            }
        }
        return "";
    }
    public static Vector3 norm(Transform v)
    {
        float x = Mathf.Round(v.eulerAngles.x);
        float y = Mathf.Round(v.eulerAngles.y);
        float z = Mathf.Round(v.eulerAngles.z);
        switch (naklon(v.eulerAngles))
        {
            case "left":
                //if ((x == 0) && (y == 0) && (z == 90));
                if ((x == 270) && (y == 90) && (z == 0))
                {
                    v.Rotate(0, 270, 0);
                }
                if ((x == 0) && (y == 180) && (z == 270))
                {
                    v.Rotate(0, 180, 0);
                }
                if ((x == 90) && (y == 270) && (z == 0))
                {
                    v.Rotate(0, 90, 0);
                }
                break;
            case "right":
                //if ((x == 0) && (y == 180) && (z == 90));
                if ((x == 270) && (y == 270) && (z == 0))
                {
                    v.Rotate(0, 270, 0);
                }
                if ((x == 0) && (y == 0) && (z == 270))
                {
                    v.Rotate(0, 180, 0);
                }
                if ((x == 90) && (y == 90) && (z == 0))
                {
                    v.Rotate(0, 90, 0);
                }
                break;
            case "forward":
                //if ((x == 0) && (y == 90) && (z == 90)) {}
                if ((x == 90) && (y == 0) && (z == 0))
                {
                    v.Rotate(0, 90, 0);
                }
                if ((x == 0) && (y == 270) && (z == 270))
                {
                    v.Rotate(0, 180, 0);
                }
                if ((x == 270) && (y == 180) && (z == 0))
                {
                    v.Rotate(0, 270, 0);
                }
                break;
            case "back":
                //if ((x == 0) && (y == 270) && (z == 90)) {}
                if ((x == 90) && (y == 180) && (z == 0))
                {
                    v.Rotate(0, 90, 0);
                }
                if ((x == 0) && (y == 90) && (z == 270))
                {
                    v.Rotate(0, 180, 0);
                }
                if ((x == 270) && (y == 0) && (z == 0))
                {
                    v.Rotate(0, 270, 0);
                }
                break;
            case "top":
                //if ((x == 0) && (y == 270) && (z == 0)) {}
                if ((x == 0) && (y == 180) && (z == 0))
                {
                    v.Rotate(0, 90, 0);
                }
                if ((x == 0) && (y == 90) && (z == 0))
                {
                    v.Rotate(0, 180, 0);
                }
                if ((x == 0) && (y == 0) && (z == 0))
                {
                    v.Rotate(0, 270, 0);
                }
                break;
            case "down":
                //if((x==0)&&(y==90)&&(z==180))print(1);
                if ((x == 0) && (y == 180) && (z == 180))
                {
                    v.Rotate(0, 90, 0);
                }
                if ((x == 0) && (y == 270) && (z == 180))
                {
                    v.Rotate(0, 180, 0);
                }
                if ((x == 0) && (y == 0) && (z == 180))
                {
                    v.Rotate(0, 270, 0);
                }
                break;
        }
        return v.eulerAngles;
    }

	static void madeJoinedPlanes2 (Transform x)
	{
		foreach (Transform child in x) {
			if (child.tag == "Plane") {
				GameObject g = Setup.PlaneAtPlane(child);
				if (g != null) {
					child.name = "Join";
					g.name = "Join";
				}
			}
		}
	}

    static void madeJoinedPlanes(Transform x)
    {
        foreach (Transform child in x)
        {
            if (child.tag == "Plane")
            {
				//GameObject g = Setup.figureAtPlane(child);
                Vector3 pos = posAtPlane(child);
                GameObject g = figureAtPos(pos);
                if (g != null)
                {
                    foreach (Transform child2 in g.transform)
                    {
                        if (child2.tag == "Plane")
                        {
                            if (Vector3.Distance(child.position, child2.position) < 0.02f)
                            {
                                child.name = "Join";
                                child2.name = "Join";
                            }
                        }
                    }
                }
                else
                {
                    child.name = "Plane";
                }
            }
        }
        return;
    }
    public static GameObject figureAtPos(Vector3 x)
    {
        if (x.y < 0)
        {
            return null;
        }
        for (int i = 0; i < GameObject.Find("Figures").transform.childCount; i++)
        {
            Transform c = GameObject.Find("Figures").transform.GetChild(i);
            if (Vector3.Distance(x, c.position) < 0.02f)
            {
                return c.gameObject;
            }
        }
        return null;
    }
    public static float fround(float x)
    {
        return Mathf.Round(x * 10) / 10;
    }
    public static int NameToInt(string n)
    {
		for(int i = 0; i < FiguresCount; i++)
		{
			if(Figures[i].Name == n)
			{
				return i;
			}
		}
		return -1;
    }
    public static string IntToName(int x)
    {
		return Figures[x].Name;
    }
    public static int ColorToInt(Color x)
    {
		for(int i=0;i<32;i++) {
			if (x == Palette [i])
				return i;
		}
        return 1;
    }
    public static Color IntToColor(int x)
    {
		return Palette [x];
    }
	public static Vector3 posAtPlane(Transform x)
	{
		Vector3 p = x.position;
		foreach (GameObject child in GameObject.FindGameObjectsWithTag("Plane"))
		{
			if (Vector3.Distance(child.transform.position, p) < 0.02f)
			{
				if(child.transform.parent.GetInstanceID()!=x.parent.GetInstanceID()) {
					return child.transform.parent.position;
				}
			}
		}
		return Vector3.zero;
	}
	public static GameObject figureAtPlane(Transform x)
	{
		Vector3 p = x.position;
		foreach (GameObject child in GameObject.FindGameObjectsWithTag("Plane"))
		{
			if (Vector3.Distance(child.transform.position, p) < 0.02f)
			{
				if(child.transform.parent.GetInstanceID()!=x.parent.GetInstanceID()) {
					return child.transform.parent.gameObject;
				}
			}
		}
		return null;
	}
	public static GameObject PlaneAtPlane(Transform x)
	{
		Vector3 p = x.position;
		foreach (GameObject child in GameObject.FindGameObjectsWithTag("Plane"))
		{
			if (Vector3.Distance(child.transform.position, p) < 0.02f)
			{
				if(child.transform.parent.GetInstanceID()!=x.parent.GetInstanceID()) {
					if (x.parent.position != child.transform.parent.position) {
						return child;
					}
				}
			}
		}
		return null;
	}

}
public class Figure
{
	private int _cpu;
	private int _sim;
	private string _name;
	private string _type;
	public int Sim{
		get {
			return _sim;
		}
	}
	public string Name{
		get {
			return _name;
		}
	}
	public int Cpu{
		get {
			return _cpu;
		}
	}
	public string Type{
		get {
			return _type;
		}
	}
	public GameObject GO;
	public Figure(
		String Folder="retro",
		String filename="beb1test2",
		int s=0,
		String name="",
		int cpu=1,
		string type="Сube"
	){
		GO = Resources.Load("Models/"+Folder+"/"+filename, typeof(GameObject)) as GameObject;
		_sim = s;
		if (name != "")
			_name = name;
		else
			_name = filename;
		_cpu = cpu;
		_type = type;
	}
	public int Faces{
		get{ 
			int i = 0;
			foreach (Transform child in GO.transform) {
				if (child.name == "Plane")
					i++;
			}
			return i;
		}
	}
}
	
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public sealed class MacaquiniFramePreviewUI : MonoBehaviour
{
    [SerializeField] private MacaquiniFrameAnimation actor;
    private Font font;
    private UnityEngine.UI.Text status, pauseLabel, speedLabel, repeatLabel;
    private UnityEngine.UI.Image[] thumbnails = new UnityEngine.UI.Image[16];
    private UnityEngine.UI.Image[] borders = new UnityEngine.UI.Image[16];
    private UnityEngine.UI.Text[] numbers = new UnityEngine.UI.Text[16];
    public Canvas PreviewCanvas { get; private set; }
    private readonly Color ink = new Color(.05f,.08f,.09f,.96f), gold = new Color(.98f,.78f,.28f), paper = new Color(.96f,.94f,.81f);
    private void Start()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            var events = new GameObject("FramePreviewEventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
        var root = new GameObject("FramePreviewCanvas", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
        PreviewCanvas = root.GetComponent<Canvas>(); PreviewCanvas.renderMode = RenderMode.ScreenSpaceOverlay; PreviewCanvas.sortingOrder = 100;
        var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1280,720); scaler.matchWidthOrHeight = .5f;
        var header = Panel(root.transform, "Header", new Vector2(0,1), new Vector2(1,1), new Vector2(0,1), new Vector2(0,70));
        var title = Text(header, "Title", "MACAQUINI  /  TESTE EM QUADROS", 24); Stretch(title.rectTransform, 24,12,24,12); title.alignment = TextAnchor.MiddleLeft;
        var info = Panel(root.transform, "Info", new Vector2(0,.5f), new Vector2(0,.5f), new Vector2(0,.5f), new Vector2(268,146)); info.anchoredPosition = new Vector2(24,34);
        status = Text(info, "Status", "", 18); Stretch(status.rectTransform,16,12,12,12); status.alignment = TextAnchor.MiddleLeft;
        var footer = Panel(root.transform, "Controls", Vector2.zero, Vector2.right, Vector2.zero, new Vector2(0,220));
        var stack = footer.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>(); stack.padding = new RectOffset(16,16,10,10); stack.spacing = 8; stack.childControlHeight = true; stack.childControlWidth = true; stack.childForceExpandHeight = false;
        var modes = Row(footer,"Modes",38);
        Button(modes,"Forma1","1 · Banana fechada",()=>actor.SetForm(false)); Button(modes,"Forma2","2 · Macaco livre",()=>actor.SetForm(true));
        Button(modes,"Swing","Balanço [B]",()=>actor.Play(MacaquiniFrameAnimation.Motion.Swing)); Button(modes,"Attack","Disparo [A]",()=>actor.Play(MacaquiniFrameAnimation.Motion.Attack)); Button(modes,"Tear","Rasgar [T]",()=>actor.Play(MacaquiniFrameAnimation.Motion.Tear));
        var transport = Row(footer,"Transport",36);
        pauseLabel = Button(transport,"Pause","Pausar [Espaço]",TogglePause).GetComponentInChildren<UnityEngine.UI.Text>();
        Button(transport,"Previous","< Quadro",()=>Step(-1)); Button(transport,"Next","Quadro >",()=>Step(1));
        Button(transport,"Slower","- Velocidade",()=>actor.Speed=Mathf.Max(.25f,actor.Speed-.25f));
        speedLabel = Button(transport,"Speed","1x",()=>actor.Speed=1).GetComponentInChildren<UnityEngine.UI.Text>();
        Button(transport,"Faster","+ Velocidade",()=>actor.Speed=Mathf.Min(2,actor.Speed+.25f));
        repeatLabel = Button(transport,"Loop","Repetir: não",()=>actor.RepeatAttack=!actor.RepeatAttack).GetComponentInChildren<UnityEngine.UI.Text>();
        var strip = Row(footer,"Frames",74);
        for(int i=0;i<16;i++)
        {
            int index=i; var button=Button(strip,"Frame"+i,"",()=>{Pause(true);actor.Seek(index);});
            borders[i]=button.GetComponent<UnityEngine.UI.Image>();
            var icon=new GameObject("Drawing",typeof(RectTransform),typeof(UnityEngine.UI.Image)); icon.transform.SetParent(button.transform,false);
            thumbnails[i]=icon.GetComponent<UnityEngine.UI.Image>();thumbnails[i].preserveAspect=true;thumbnails[i].raycastTarget=false;Stretch(icon.GetComponent<RectTransform>(),2,2,2,16);
            numbers[i]=button.GetComponentInChildren<UnityEngine.UI.Text>();numbers[i].fontSize=12;numbers[i].alignment=TextAnchor.LowerCenter;
        }
        var hint=Text(footer,"Hint","Corpo, cipó, arma, rosto, pernas, pés e rabo no mesmo desenho. Clique num quadro para examinar.",13);
        hint.alignment=TextAnchor.MiddleCenter;hint.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight=17;
    }
    private void Update()
    {
        var keys=Keyboard.current;if(keys==null)return;
        if(keys.spaceKey.wasPressedThisFrame)TogglePause();
        if(keys.leftArrowKey.wasPressedThisFrame)Step(-1);if(keys.rightArrowKey.wasPressedThisFrame)Step(1);
        if(keys.digit1Key.wasPressedThisFrame)actor.SetForm(false);if(keys.digit2Key.wasPressedThisFrame)actor.SetForm(true);
        if(keys.bKey.wasPressedThisFrame)actor.Play(MacaquiniFrameAnimation.Motion.Swing);if(keys.aKey.wasPressedThisFrame)actor.Play(MacaquiniFrameAnimation.Motion.Attack);if(keys.tKey.wasPressedThisFrame)actor.Play(MacaquiniFrameAnimation.Motion.Tear);
        if(keys.escapeKey.wasPressedThisFrame)Application.Quit();
    }
    private void LateUpdate()
    {
        if(status==null)return;
        string motion=actor.CurrentMotion==MacaquiniFrameAnimation.Motion.Swing?"BALANÇO":actor.CurrentMotion==MacaquiniFrameAnimation.Motion.Attack?"DISPARO":"RASGANDO A BANANA";
        status.text=(actor.HalfShell?"FORMA 2 · MACACO":"FORMA 1 · BANANA")+"\n"+motion+"\nQUADRO "+(actor.Frame+1).ToString("D2")+" / "+actor.CurrentFrames.Length+"\n56 desenhos completos";
        pauseLabel.text=actor.Paused?"Reproduzir [Espaço]":"Pausar [Espaço]";speedLabel.text=actor.Speed.ToString("0.##")+"x";
        repeatLabel.text=actor.RepeatAttack?"Repetir: sim":"Repetir: não";
        for(int i=0;i<16;i++){bool active=i<actor.CurrentFrames.Length;borders[i].gameObject.SetActive(active);if(active){thumbnails[i].sprite=actor.CurrentFrames[i];numbers[i].text=(i+1).ToString("D2");borders[i].color=i==actor.Frame?gold:new Color(.14f,.21f,.17f,1);}}
    }
    public void TogglePause()=>Pause(!actor.Paused);
    public void Pause(bool paused){actor.Paused=paused;Time.timeScale=paused?0:1;}
    public void Step(int offset){Pause(true);actor.Seek(actor.Frame+offset);}
    private void OnDisable(){Time.timeScale=1;}
    private RectTransform Panel(Transform parent,string name,Vector2 min,Vector2 max,Vector2 pivot,Vector2 size)
    {
        var node=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image));node.transform.SetParent(parent,false);var rect=node.GetComponent<RectTransform>();rect.anchorMin=min;rect.anchorMax=max;rect.pivot=pivot;rect.sizeDelta=size;node.GetComponent<UnityEngine.UI.Image>().color=ink;return rect;
    }
    private Transform Row(Transform parent,string name,float height)
    {
        var row=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.HorizontalLayoutGroup),typeof(UnityEngine.UI.LayoutElement));row.transform.SetParent(parent,false);row.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=height;
        var layout=row.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();layout.spacing=6;layout.childControlWidth=layout.childControlHeight=true;layout.childForceExpandWidth=true;return row.transform;
    }
    private UnityEngine.UI.Button Button(Transform parent,string name,string label,UnityAction action)
    {
        var node=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image),typeof(UnityEngine.UI.Button),typeof(UnityEngine.UI.LayoutElement));node.transform.SetParent(parent,false);
        var image=node.GetComponent<UnityEngine.UI.Image>();image.color=new Color(.14f,.21f,.17f,1);var button=node.GetComponent<UnityEngine.UI.Button>();button.targetGraphic=image;button.onClick.AddListener(action);
        var colors=button.colors;colors.highlightedColor=new Color(.8f,1,.7f);colors.pressedColor=gold;button.colors=colors;node.GetComponent<UnityEngine.UI.LayoutElement>().flexibleWidth=1;
        var text=Text(node.transform,"Label",label,15);Stretch(text.rectTransform,4,2,4,2);text.alignment=TextAnchor.MiddleCenter;return button;
    }
    private UnityEngine.UI.Text Text(Transform parent,string name,string value,int size)
    {
        var node=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Text));node.transform.SetParent(parent,false);var text=node.GetComponent<UnityEngine.UI.Text>();text.font=font;text.fontSize=size;text.text=value;text.color=paper;text.raycastTarget=false;return text;
    }
    private static void Stretch(RectTransform rect,float left,float top,float right,float bottom){rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=new Vector2(left,bottom);rect.offsetMax=new Vector2(-right,-top);}
}

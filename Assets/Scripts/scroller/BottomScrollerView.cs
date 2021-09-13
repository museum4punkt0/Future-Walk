using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DigitalRuby.Tween;
using UnityEngine.EventSystems;
using System.Text;

public class BottomScrollerView : UIBehaviour, IPointerUpHandler, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler
{
    [Header("Scroller View")]
    [SerializeField] float spacing = 10;
    [SerializeField] RectTransform cellContainerWrapper; // contains cellContainer. it gets moved by scrolling up/down
    [SerializeField] RectTransform cellContainer; // individual cells are in here. it gets pushed up for new cells
    [SerializeField] GameObject scrollDownButton;
    [SerializeField] Text debugText;
    [SerializeField] float topOffset = 0;
    [SerializeField] float animationDuration = 0.3f;

    [Header("Chat Bubbles")]
    [SerializeField] GameObject motokoTextBubble;
    [SerializeField] GameObject motokoAudioBubble;
    [SerializeField] GameObject andreaAudioBubble;
    [SerializeField] GameObject andreaTextBubble;
    [SerializeField] GameObject playerBubble;
    [SerializeField] GameObject imageBubble;
    [SerializeField] GameObject mapBubble;
    [SerializeField] GameObject artBubble;


    [Header("Scroller")]
    [SerializeField] bool invertScroll = true;
    [SerializeField] float deaccelerationRate = 0.8f;

    // scroller
    public float velocity;
    Vector2 beginDragPointerPosition;
    float lastDragDelta;
    bool dragging;


    // scroller view
    private List<ChatEntry> entries = new List<ChatEntry>();
    private List<StoryChatCell> cells = new List<StoryChatCell>();


    float currentPosition = 0;
    float lastPosition = 0;

    float realOriginalPosition = 0;
    float originalPosition = 0;
    float scrollerHeight = 0;
    float fillUpHeight = 0;

    int upperIndex = -1;

    float upperPosition = 0; // height above 0
    float lowerPosition = 0; // height below 0

    // wether we need to wait until loading/animation is finished
    bool cellInProcess = false; 

    bool isEnabled = false;

    // action to be called after adding a cell (animation) is done
    Action onCellAnimationDone = null;

    // a backup coroutine to make sure we call AnimationDone()
    Coroutine bkup = null;

    // flag to enable/disable scrolling
    bool scrollingEnabled = true;
    public bool SetEnableScrolling
    {
        set => scrollingEnabled = value;
    }


    // calculate how much we can scroll
    float GetEnd() => upperPosition + Mathf.Abs(lowerPosition) - scrollerHeight + spacing + topOffset;
    float end;

    //------------------------------------------------------------
    // backlog
    class BacklogItem
    {
        public ChatEntry entry;
        public Action finishAction = null;
        public bool push;
    }
    Queue<BacklogItem> backlogStack = new Queue<BacklogItem>();


    //------------------------------------------------------------
    //------------------------------------------------------------
    protected override void Awake()
    {
        scrollDownButton?.SetActive(false);
    }

    protected override void Start()
    {
        // we should have got a height
        GetHeight();
    }


    private void GetHeight()
    {
        originalPosition = cellContainerWrapper.localPosition.y;
        realOriginalPosition = originalPosition;

        scrollerHeight = GetComponent<RectTransform>().rect.height;
        fillUpHeight = scrollerHeight * 3;

        cellContainerWrapper.localPosition = new Vector3(
            cellContainerWrapper.localPosition.x,
            originalPosition - currentPosition,
            cellContainerWrapper.localPosition.z);
    }


    private void OnDisable()
    {
        isEnabled = false;

        // NOTE:
        // all corountines are getting stopped
        // make sure this flag is false if no coroutines are running!
        cellInProcess = false;
    }

    private void OnEnable()
    {
        isEnabled = true;

        if (backlogStack.Count > 0)
        {            
            BacklogItem backlog = backlogStack.Dequeue();
            AddItem(backlog.entry, backlog.finishAction);
        }
    }

    private void Update()
    {
        // NOTE: in case layouting did not finish??
        // do we need this?
        if (scrollerHeight <= 1)
        {
            GetHeight();
            return;
        }


        //--------------------------------------------
        // check if we need to create a cell above
        if (!cellInProcess &&
            upperIndex >= 0 &&
            (upperPosition - currentPosition - lowerPosition) < fillUpHeight)
        {
            cellInProcess = true;

            var entry = entries[upperIndex];
            
            var cell = CreateCell(entry.type);
            if (cell)
            {
                // add at end of list
                cells.Add(cell);

                cell.LoadContentAsync(entry, (bool state) =>
                {
                    // TODO: should we check the state?
                    // NOTE: cell.UpdateContent would try to load from Resources
                    // CHECK: needed?

                    // update content
                    cell.UpdateContent(entry);
                    
                    StartCoroutine(WaitForLayoutUpper(cell));
                });
                //StartCoroutine(LoadContentShowDirect(cell, entries[upperIndex]));

                // next upper index
                upperIndex--;
            }
            else
            {
                // no cell
                AnimationDone();
            }
        }

        //--------------------------------------------
        // check if we can remove some cells
        while (cells.Count > 0 &&
            (upperPosition - lowerPosition) > fillUpHeight &&
            (upperPosition - currentPosition - lowerPosition) > (fillUpHeight + 400))
        {
            // remove last entry from pool and destroy it
            var child = cells[cells.Count - 1];
            cells.RemoveAt(cells.Count - 1);

            upperPosition -= (child.Height() + spacing);
            upperIndex++;

            Destroy(child.gameObject);

            if (upperIndex >= entries.Count)
            {
                // how can this happen?
                upperIndex = entries.Count - 1;
                Log.d("correct upper index to: " + upperIndex);
                break;
            }
        }


        ///
        /// update position releated to velocity
        ///


        if (Mathf.Abs(velocity) < 0.01f)
        {
            // stop
            velocity = 0;
        }
        else
        {
            currentPosition += velocity;
            velocity *= deaccelerationRate;
        }

        // constrain position

        // get upper end
        end = GetEnd();

        if (currentPosition > end)
        {
            currentPosition = end;
            velocity = 0;
        }
        if (currentPosition < 0)
        {
            currentPosition = 0;
            velocity = 0;
        }

        if (!Mathf.Approximately(currentPosition, lastPosition))
        {
            //if (scrollDownButton)
            //{
            //    if (currentPosition > 0 &&
            //        currentPosition < end &&
            //        currentPosition - lastPosition < 0)
            //    {
            //        if (currentPosition - lastPosition < 10)
            //        {
            //            scrollDownButton.SetActive(true);
            //        }
            //    }
            //    else
            //    {
            //        scrollDownButton.SetActive(false);
            //    }
            //}

            // update position of container wrapper            
            cellContainerWrapper.localPosition = new Vector3(
                cellContainerWrapper.localPosition.x,
                originalPosition - currentPosition,
                cellContainerWrapper.localPosition.z);


            lastPosition = currentPosition;

            if (debugText)
            {
                debugText.text =
                    "p: " + currentPosition + "\n" +
                    "e: " + end + "\n" +
                    "";
            }
        }

        if (currentPosition > 2)
        {
            scrollDownButton?.SetActive(true);
        }
        else
        {
            scrollDownButton?.SetActive(false);
        }

    }


    //------------------------------------------------------------
    //------------------------------------------------------------
    public void ScrollToEnd()
    {
        velocity = 0f;
        currentPosition = 0;
    }


    public void ClearContent()
    {
        SetContent(new List<ChatEntry>());
    }


    public void SetContent(List<ChatEntry> content)
    {
        StopAllCoroutines();
        bkup = default;

        foreach (var cell in cells)
        {
            Destroy(cell.gameObject);
        }
        cells.Clear();

        entries = content;
        upperIndex = entries.Count - 1;
        upperPosition = 0;
        lowerPosition = 0;

        velocity = 0f;
        currentPosition = 0;

        originalPosition = realOriginalPosition;

        // set cell container to y = 0
        cellContainer.localPosition = new Vector3(
            cellContainer.localPosition.x,
            0,
            cellContainer.localPosition.z);

        scrollDownButton?.SetActive(false);
    }


    //------------------------------------------------------------
    //------------------------------------------------------------
    // add a new item at the bottom
    //------------------------------------------------------------
    //------------------------------------------------------------

    public void AddItem(ChatEntry entry, Action finishAction = null)
    {
        if (entry == null) return;

        if (cellInProcess || !isEnabled)
        {
            BacklogItem item = new BacklogItem();
            item.entry = entry;
            item.finishAction = finishAction;

            backlogStack.Enqueue(item);

            Log.d("animation is running - backlog size: " + backlogStack.Count + " -- " + entry.content);
            return;
        }


        //----------------------------------------------------
        // no other cell in process
        // set flag to busy
        // NOTE: make sure we are not getting stuck with this variable set to true
        cellInProcess = true;

        // set finish action
        onCellAnimationDone = finishAction;

        // add to list of entries
        entries.Add(entry);

        //Log.d("add item: " + entry.content);

        try
        {
            // create new cell
            var cell = CreateCell(entry.type);


            //Canvas c = cell.GetComponent<Canvas>(true);
            //var cr = CanvasUpdateRegistry.instance;

            //LayoutRebuilder.MarkLayoutForRebuild(cell.GetComponent<RectTransform>());


            if (cell)
            {
                // insert cell at beginning
                cells.Insert(0, cell);

                // start backup timer
                bkup = StartCoroutine(BackupTimer());

                cell.LoadContentAsync(entry, (bool result) =>
                {
                    // TODO: check for result?
                    // -> this would skip this bubble - do we want that?
                    // -> if we don't check and cells might show with wrong content
                    cell.UpdateContent(entry);

                    StartCoroutine(WaitForLayoutLower(cell));
                });
            }
            else
            {
                PostTweenAction();
            }
        }
        catch(Exception e)
        {
            // try-catch needed?
            PostTweenAction();
        }
    }


    //------------------------------------------------------------
    //------------------------------------------------------------
    private StoryChatCell CreateCell(ChatEntry.ChatEntryType type)
    {
        GameObject cell = default;

        if (motokoTextBubble && type == ChatEntry.ChatEntryType.Bot)
        {
            cell = Instantiate(motokoTextBubble, cellContainer.transform);
        }
        else if (motokoAudioBubble && type == ChatEntry.ChatEntryType.BotAudio)
        {
            cell = Instantiate(motokoAudioBubble, cellContainer.transform);
        }
        else if (andreaTextBubble && type == ChatEntry.ChatEntryType.Andrea)
        {
            cell = Instantiate(andreaTextBubble, cellContainer.transform);
        }
        else if (andreaAudioBubble && type == ChatEntry.ChatEntryType.AndreaAudio)
        {
            cell = Instantiate(andreaAudioBubble, cellContainer.transform);
        }
        else if (playerBubble && type == ChatEntry.ChatEntryType.Player)
        {
            cell = Instantiate(playerBubble, cellContainer.transform);
        }
        else if (imageBubble && type == ChatEntry.ChatEntryType.Image)
        {
            cell = Instantiate(imageBubble, cellContainer.transform);
        }
        else if (mapBubble && type == ChatEntry.ChatEntryType.Map)
        {
            cell = Instantiate(mapBubble, cellContainer.transform);
        }
        else if (artBubble && type == ChatEntry.ChatEntryType.Artwork)
        {
            cell = Instantiate(artBubble, cellContainer.transform);
        }
        

        if (cell)
        {
            var c = cell.GetComponent<StoryChatCell>();

            c.UpdatePositionXY(0, 10000);

            return c;
        }

        // error
        Log.d("could not create cell for type: " + type);

        return default;
    }


    private void AnimationDone()
    {
        if (bkup != null)
        {
            StopCoroutine(bkup);
            bkup = null;
        }

        // Log.d("antimation done - backlog:: " + backlogStack.Count);

        cellInProcess = false;

        if (backlogStack.Count > 0)
        {
            BacklogItem backlog = backlogStack.Dequeue();

            //Log.d("add from cache - left: " + backlogStack.Count);

            AddItem(backlog.entry, backlog.finishAction);
        }
    }


    // NOTE:
    // in case something goes wrong in the animaton or in one of the coroutines
    // we need to make sure we do not get stuck with cellIntroBusy == true;
    //
    // be aware: disabling this Monobehaviour can render this strategy useless as all coroutines would stop!
    //
    IEnumerator BackupTimer()
    {
        yield return new WaitForSeconds(animationDuration + 1);

        if (onCellAnimationDone != null)
        {
            Action action = onCellAnimationDone;
            onCellAnimationDone = null;
            action();
        }

        AnimationDone();
    }


    // wait until content is loaded and start animation
    IEnumerator LoadContentShowDirect(StoryChatCell cell, ChatEntry entry)
    {
        yield return cell.LoadContent(entry);

        try
        {
            // update content
            cell.UpdateContent(entry);
            cell.gameObject.SetActive(true);
        }
        catch (Exception e)
        {
            // nop...
            Log.d("LoadContentShowDirect - exception: " + e);
        }

        // wait for layouting
        cell.gameObject.SetActive(true);

        if (cell.ForceRelayout())
        {
            while (!cell.LayoutDone || !cell.GraphicDone)
            {
                yield return null;
            }
        }
        else
        {
            // at least wait a frame...
            yield return null;
        }

        if (checkFitter(cell))
        {
            if (cell.ForceRelayout())
            {
                while (!cell.LayoutDone || !cell.GraphicDone)
                {
                    yield return null;
                }
            }
            else
            {
                // at least wait a frame...
                yield return null;
            }
        }


        try
        {
            // set it at below bottom
            upperPosition += cell.Height() + spacing;
            cell.UpdatePositionXY(0, upperPosition);
        }
        finally
        {
            AnimationDone();
        }

    }

    IEnumerator WaitForLayoutUpper(StoryChatCell cell)
    {
        // wait for layouting
        cell.gameObject.SetActive(true);

        if (cell.ForceRelayout())
        {
            while (!cell.LayoutDone || !cell.GraphicDone)
            {
                yield return null;
            }
        }
        else
        {
            // at least wait a frame...
            yield return null;
        }

        if (checkFitter(cell))
        {
            if (cell.ForceRelayout())
            {
                while (!cell.LayoutDone || !cell.GraphicDone)
                {
                    yield return null;
                }
            }
            else
            {
                // at least wait a frame...
                yield return null;
            }
        }

        // layout should be done

        try
        {
            //Log.d("upper: " + cell.Content + " - height: " + cell.Height());

            upperPosition += cell.Height() + spacing;
            cell.UpdatePositionXY(0, upperPosition);
        }
        finally
        {
            AnimationDone();
        }
    }


    bool checkFitter(StoryChatCell cell)
    {        
        var c = cell.GetComponentInChildren<WidthConstraint>();
        if (c)
        {
            return c.Constrain();
        }

        return false;
    }
    
    IEnumerator WaitForLayoutLower(StoryChatCell cell)
    {
        cell.gameObject.SetActive(true);

        if (cell.ForceRelayout())
        {
            while (!cell.LayoutDone || !cell.GraphicDone)
            {
                yield return null;
            }
        }
        else
        {
            // at least wait a frame...
            yield return null;
        }

        if (checkFitter(cell))
        {
            if (cell.ForceRelayout())
            {
                while (!cell.LayoutDone || !cell.GraphicDone)
                {
                    yield return null;
                }
            }
            else
            {
                // at least wait a frame...
                yield return null;
            }
        }

        //------------------------
        // layout should be done

        try
        {
            // cell height + spacing
            float cellHeight = cell.Height() + spacing;
            // place it
            cell.UpdatePositionXY(0, lowerPosition);
            lowerPosition -= cellHeight;

            float last = 0;
            Action<ITween<float>> moveUp = (t) =>
            {
                // add up animation-value differences
                // we need to take active scrolling into account
                cellContainer.localPosition += new Vector3(0, (t.CurrentValue - last), 0);
                last = t.CurrentValue;
            };

            Action<ITween<float>> moveUpCompleted = (t) =>
            {
                PostTweenAction();
            };


            if (Mathf.Approximately(cellContainerWrapper.localPosition.y, originalPosition))
            {
                gameObject.Tween("moveUp",
                            0,
                            cellHeight,
                            animationDuration,
                            TweenScaleFunctions.CubicEaseInOut,
                            moveUp,
                            moveUpCompleted);
            }
            else
            {
                // cell is added at bottom
                // correct container offsets and tell scroller
                originalPosition += cellHeight;
                currentPosition += cellHeight;
                lastPosition = currentPosition;

                scrollDownButton?.SetActive(true);

                PostTweenAction();
            }
        }
        catch
        {
            PostTweenAction();
        }

    }

    private void PostTweenAction()
    {
        AnimationDone();

        if (onCellAnimationDone != null)
        {
            Action action = onCellAnimationDone;
            onCellAnimationDone = null;
            action();
        }
    }





    //------------------------------------------------------------
    //------------------------------------------------------------
    // scroller
    //------------------------------------------------------------
    //------------------------------------------------------------

    /// <inheritdoc/>
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        velocity = 0f;
    }

    /// <inheritdoc/>
    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }
    }

    /// <inheritdoc/>
    void IScrollHandler.OnScroll(PointerEventData eventData)
    {
        if (!scrollingEnabled) return;

        var scrollDelta = eventData.scrollDelta.y * (invertScroll ? 1 : -1);

        currentPosition += scrollDelta;
    }

    /// <inheritdoc/>
    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        if (!scrollingEnabled || eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out beginDragPointerPosition);

        dragging = true;
        lastDragDelta = 0;
    }

    /// <inheritdoc/>
    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        if (!scrollingEnabled || eventData.button != PointerEventData.InputButton.Left || !dragging)
        {
            return;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out var dragPointerPosition))
        {
            return;
        }

        var diff = dragPointerPosition - beginDragPointerPosition;
        beginDragPointerPosition = dragPointerPosition;

        var scrollDelta = diff.y * (invertScroll ? -1 : 1);

        lastDragDelta = scrollDelta;
        currentPosition += scrollDelta;
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        if (!scrollingEnabled || eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (Mathf.Abs(lastDragDelta) > 10)
        {
            // set auto scrolling velocity
            velocity += lastDragDelta;
        }

        lastDragDelta = 0;
        dragging = false;
    }

}


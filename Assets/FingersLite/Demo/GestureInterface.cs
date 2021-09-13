//
// Fingers Gestures
// (c) 2015 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

namespace DigitalRubyShared
{
    public class GestureInterface : MonoBehaviour
    {
        public GameObject target;

        private TapGestureRecognizer tapGesture;
        private TapGestureRecognizer doubleTapGesture;
        private TapGestureRecognizer tripleTapGesture;
        private SwipeGestureRecognizer swipeGesture;
        private PanGestureRecognizer panGesture;
        private ScaleGestureRecognizer scaleGesture;
        private RotateGestureRecognizer rotateGesture;
        private LongPressGestureRecognizer longPressGesture;

        private GameObject draggingGameObject;

        Coroutine returner;

        [SerializeField] Camera cam;

        [SerializeField] bool returning = false;
        [SerializeField] float returnTime = 0.1f;
        [SerializeField] float returnSpeed = 1.0f;
        [SerializeField] float holdToDragThreshold = 0.06f;
        [SerializeField] bool portrait = false;
        float minScale, maxScale;


        private void DebugText(string text, params object[] format)
        {
            //Debug.Log(string.Format(text, format));
        }


        private void BeginDrag(float screenX, float screenY)
        {
            Vector3 pos = new Vector3(screenX, screenY, 0.0f);
            pos = Camera.main.ScreenToWorldPoint(pos);
            RaycastHit2D hit = Physics2D.CircleCast(pos, 10.0f, Vector2.zero);
            if (hit.transform != null && hit.transform.gameObject == target)
            {
                draggingGameObject = hit.transform.gameObject;
            }
            else
            {
                longPressGesture.Reset();
            }
        }

        private void DragTo(float screenX, float screenY)
        {
            if (draggingGameObject == null)
            {
                return;
            }
            Vector3 pos = new Vector3(screenX, screenY, 0.0f);
            pos = Camera.main.ScreenToWorldPoint(pos);
            draggingGameObject.GetComponent<Rigidbody2D>().MovePosition(pos);
        }

        private void EndDrag(float velocityXScreen, float velocityYScreen)
        {
            if (draggingGameObject == null)
            {
                return;
            }

            Vector3 origin = Camera.main.ScreenToWorldPoint(Vector3.zero);
            Vector3 end = Camera.main.ScreenToWorldPoint(new Vector3(velocityXScreen, velocityYScreen, 0.0f));
            Vector3 velocity = (end - origin);
            draggingGameObject.GetComponent<Rigidbody2D>().velocity = velocity;
            draggingGameObject.GetComponent<Rigidbody2D>().angularVelocity = UnityEngine.Random.Range(5.0f, 45.0f);
            draggingGameObject = null;

            DebugText("Long tap flick velocity: {0}", velocity);
        }

        private void HandleSwipe(float endX, float endY)
        {
            Vector2 start = new Vector2(swipeGesture.StartFocusX, swipeGesture.StartFocusY);
            Vector3 startWorld = Camera.main.ScreenToWorldPoint(start);
            Vector3 endWorld = Camera.main.ScreenToWorldPoint(new Vector2(endX, endY));
            float distance = Vector3.Distance(startWorld, endWorld);
            startWorld.z = endWorld.z = 0.0f;
        }

        private void TapGestureCallback(GestureRecognizer gesture)
        {
            return;
        }

        private void CreateTapGesture()
        {
            tapGesture = new TapGestureRecognizer();
            tapGesture.StateUpdated += TapGestureCallback;
            tapGesture.RequireGestureRecognizerToFail = doubleTapGesture;
            FingersScript.Instance.AddGesture(tapGesture);
        }

        private void DoubleTapGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                DebugText("Double tapped at {0}, {1}", gesture.FocusX, gesture.FocusY);

                //todo: zoom in zoom out
            }
        }

        private void CreateDoubleTapGesture()
        {
            doubleTapGesture = new TapGestureRecognizer();
            doubleTapGesture.NumberOfTapsRequired = 2;
            doubleTapGesture.StateUpdated += DoubleTapGestureCallback;
            doubleTapGesture.RequireGestureRecognizerToFail = tripleTapGesture;
            FingersScript.Instance.AddGesture(doubleTapGesture);
        }

        private void SwipeGestureCallback(GestureRecognizer gesture)
        {
            return;
        }

        private void CreateSwipeGesture()
        {
            swipeGesture = new SwipeGestureRecognizer();
            swipeGesture.Direction = SwipeGestureRecognizerDirection.Any;
            swipeGesture.StateUpdated += SwipeGestureCallback;
            swipeGesture.DirectionThreshold = 1.0f; // allow a swipe, regardless of slope
            FingersScript.Instance.AddGesture(swipeGesture);
        }

        private void PanGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                DebugText("Panned, Location: {0}, {1}, Delta: {2}, {3}", gesture.FocusX, gesture.FocusY, gesture.DeltaX, gesture.DeltaY);

                float deltaX = panGesture.DeltaX;
                float deltaY = panGesture.DeltaY;
                Vector3 pos = target.transform.position;
                
                pos.x += deltaX;
                pos.y += deltaY;
                target.transform.position = pos;
            }
        }

        private void CreatePanGesture()
        {
            panGesture = new PanGestureRecognizer();
            panGesture.MinimumNumberOfTouchesToTrack = 2;
            panGesture.StateUpdated += PanGestureCallback;
            FingersScript.Instance.AddGesture(panGesture);
        }

        private void ScaleGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                DebugText("Scaled: {0}, Focus: {1}, {2}", scaleGesture.ScaleMultiplier, scaleGesture.FocusX, scaleGesture.FocusY);
                var s = target.transform.localScale.x * scaleGesture.ScaleMultiplier;
                s = Mathf.Clamp(s, minScale, maxScale);
                target.transform.localScale = new Vector3(s,s,0f);
            }
        }

        private void CreateScaleGesture()
        {
            scaleGesture = new ScaleGestureRecognizer();
            scaleGesture.StateUpdated += ScaleGestureCallback;
            FingersScript.Instance.AddGesture(scaleGesture);
        }

        private void RotateGestureCallback(GestureRecognizer gesture)
        {
            return;
        }

        private void CreateRotateGesture()
        {
            rotateGesture = new RotateGestureRecognizer();
            rotateGesture.StateUpdated += RotateGestureCallback;
            FingersScript.Instance.AddGesture(rotateGesture);
        }

        private void LongPressGestureCallback(GestureRecognizer gesture)
        {

            if (gesture.State == GestureRecognizerState.Executing)
            {
                DebugText("Panned, Location: {0}, {1}, Delta: {2}, {3}", gesture.FocusX, gesture.FocusY, gesture.DeltaX, gesture.DeltaY);

                float deltaX = panGesture.DeltaX;
                float deltaY = panGesture.DeltaY;
                Vector3 pos = target.transform.position;
                
                pos.x += deltaX;
                pos.y += deltaY;
                target.transform.position = pos;
                
            }
        }

        private void CreateLongPressGesture()
        {
            longPressGesture = new LongPressGestureRecognizer();
            longPressGesture.MaximumNumberOfTouchesToTrack = 1;
            longPressGesture.StateUpdated += LongPressGestureCallback;
            FingersScript.Instance.AddGesture(longPressGesture);
        }

        private void PlatformSpecificViewTapUpdated(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                Debug.Log("You triple tapped the platform specific label!");
            }
        }

        private void CreatePlatformSpecificViewTripleTapGesture()
        {
            tripleTapGesture = new TapGestureRecognizer();
            tripleTapGesture.StateUpdated += PlatformSpecificViewTapUpdated;
            tripleTapGesture.NumberOfTapsRequired = 3;
            //tripleTapGesture.PlatformSpecificView = bottomLabel.gameObject;
            FingersScript.Instance.AddGesture(tripleTapGesture);
        }

        private static bool? CaptureGestureHandler(GameObject obj)
        {
            // I've named objects PassThrough* if the gesture should pass through and NoPass* if the gesture should be gobbled up, everything else gets default behavior
            if (obj.name.StartsWith("PassThrough"))
            {
                // allow the pass through for any element named "PassThrough*"
                return false;
            }
            else if (obj.name.StartsWith("NoPass"))
            {
                // prevent the gesture from passing through, this is done on some of the buttons and the bottom text so that only
                // the triple tap gesture can tap on it
                return true;
            }

            // fall-back to default behavior for anything else
            return null;
        }

        private void OnEnable() 
        {
            // cam.orthographicSize = 0.5f*(float)Screen.height;
            // Vector2 imageSize = target.GetComponent<SpriteRenderer>().sprite.rect.size;

            // Debug.Log("Screen Size: "+ Screen.width +", "+ Screen.height);
            // Debug.Log("sprite size:" + imageSize);

            // float scaleH = Screen.width/imageSize.x*100f;
            // float scaleV = Screen.height/imageSize.y*100f;

            // minScale = scaleH;
            // maxScale = 2.5f*minScale;

            // float scale = portrait? scaleV : scaleH;

            // //Vector3 sc = new Vector3(scale, scale, 0f);

            // target.transform.position = new Vector3(0f, 0f, 0f);
            // target.transform.localScale = new Vector3(scale, scale, 0f); 
        }

        
        // public Vector4 _imgSize_border;
        // public Vector3 _imgSize_bounds;
        // public Vector2 _imgSize_rect;
        // public Vector2 _imgSize_textureRect;
        
        

        public void Reset()
        {
            //Debug.Log("TODO ResetScript");
            cam.orthographicSize = 0.5f*(float)Screen.height;

            // Vector3 imgBounds = target.GetComponent<SpriteRenderer>().sprite.bounds.size;
            float ppu = target.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit;
            // Vector2 imageSize = new Vector2(imgBounds.x * ppu, imgBounds.y * ppu);

            Vector2 imageSize = new Vector2(target.GetComponent<SpriteRenderer>().sprite.bounds.size.x * ppu,
                                            target.GetComponent<SpriteRenderer>().sprite.bounds.size.y * ppu);


            float scaleH = Screen.width/imageSize.x * ppu;
            float scaleV = Screen.height/imageSize.y * ppu;

            minScale = scaleH;
            maxScale = 2.5f*minScale;

            float scale = portrait? scaleV : scaleH;

            //Vector3 sc = new Vector3(scale, scale, 0f);

            target.transform.position = new Vector3(0f, 0f, 0f);
            target.transform.localScale = new Vector3(scale, scale, 0f);


            // _imgSize_border = target.GetComponent<SpriteRenderer>().sprite.border;
            // _imgSize_bounds = target.GetComponent<SpriteRenderer>().sprite.bounds.size;
            // _imgSize_rect = target.GetComponent<SpriteRenderer>().sprite.rect.size;
            // _imgSize_textureRect = target.GetComponent<SpriteRenderer>().sprite.textureRect.size;            
        }

        private void Start()
        {

            // don't reorder the creation of these :)
            CreatePlatformSpecificViewTripleTapGesture();
            CreateDoubleTapGesture();
            CreateTapGesture();
            CreateSwipeGesture();
            CreatePanGesture();
            CreateScaleGesture();
            CreateRotateGesture();
            CreateLongPressGesture();

            longPressGesture.MinimumDurationSeconds = holdToDragThreshold;

            // pan, scale and rotate can all happen simultaneously
            panGesture.AllowSimultaneousExecution(scaleGesture);
            panGesture.AllowSimultaneousExecution(rotateGesture);
            scaleGesture.AllowSimultaneousExecution(rotateGesture);

            // prevent the one special no-pass button from passing through,
            //  even though the parent scroll view allows pass through (see FingerScript.PassThroughObjects)
            FingersScript.Instance.CaptureGestureHandler = CaptureGestureHandler;
        }

        private void Update() 
        {
            if(FingersScript.Instance.Touches.Count == 0)
            {
                if(!returning)
                {
                    CheckBBox();
                }
                
            }
            else
            {
                returning = false;
                returner = default;
                StopAllCoroutines();
            }
        }

        // public Vector2 _imgs;
        // public Vector2 _ss;
        // public Vector3 _p;
        // public float _deltax;

        private void CheckBBox()
        {
            
            Vector2 ss = new Vector2(Screen.width, Screen.height);

            //Vector2 imgs = target.GetComponent<SpriteRenderer>().sprite.rect.size;
            //imgs = new Vector2(imgs.x*target.transform.localScale.x/100f, imgs.y*target.transform.localScale.y/100f);

            float ppu = target.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit;

            Vector2 imgs = new Vector2(target.GetComponent<SpriteRenderer>().sprite.bounds.size.x * ppu,
                                        target.GetComponent<SpriteRenderer>().sprite.bounds.size.y * ppu);


            imgs *= target.transform.localScale.x/ppu;

            Vector3 p = target.transform.position;
            Vector3 end = new Vector3(0.0f,0.0f,0.0f);

            // _imgs=imgs;
            // _ss=ss;
            // _p=p;

            // HORIZONTAL BBOX ADJUST
            if(imgs.x <= ss.x && !returning)
            {
                // if image is the size of the screen, adjust to center
                end.x = 0f;
            }
            else
            {
                // if image is wider than the screen, let new position be img position and adjust border
                //end.x = p.x;

                if(p.x < 0f && 0.5f * imgs.x + p.x < ss.x /*&& !returning*/)
                {
                    // adjust to right border of the image if no gesture is detected
                    // && there is an empty space between the image and the right border

                    //Debug.Log("too much to the LEFT");

                    //float deltax = 0.5f * ss.x - (0.5f * imgs.x + p.x);
                    //end.x = p.x+deltax;
                    //end.x += deltax;
                    end.x = p.x + 0.5f * ss.x - (0.5f * imgs.x + p.x);
                }

                if(p.x > 0f && 0.5f * imgs.x - p.x < ss.x /*&& !returning*/)
                {
                    // adjust to left border of the image. same as above
                    //Debug.Log("too much to the RIGHT");

                    //float deltax = -0.5f * ss.x - ( -0.5f * imgs.x + p.x);
                    end.x = p.x -0.5f * ss.x - ( -0.5f * imgs.x + p.x); 
                    //end.x += deltax;
                }

                if(Mathf.Abs(p.x)< (Mathf.Abs(0.5f * imgs.x) - Mathf.Abs(0.5f * ss.x)))
                {
                    end.x = p.x;
                }
            }

            // VERTICAL BBOX ADJUST
            if(imgs.y <= ss.y && !returning)
            {
                // if image is the size of the screen, adjust to center
                end.y = 0f;
            }
            else
            {
                // if image is wider than the screen, let new position be img position and adjust border
                //end.x = p.x;

                if(p.y < 0f && 0.5f * imgs.y + p.y < ss.y /*&& !returning*/)
                {
                    // adjust to right border of the image if no gesture is detected
                    // && there is an empty space between the image and the right border

                    //Debug.Log("too much to the LEFT");

                    //float deltax = 0.5f * ss.x - (0.5f * imgs.x + p.x);
                    //end.x = p.x+deltax;
                    //end.x += deltax;
                    end.y = p.y + 0.5f * ss.y - (0.5f * imgs.y + p.y);
                }

                if(p.y > 0f && 0.5f * imgs.y - p.y < ss.y /*&& !returning*/)
                {
                    // adjust to left border of the image. same as above
                    //Debug.Log("too much to the RIGHT");

                    //float deltax = -0.5f * ss.x - ( -0.5f * imgs.x + p.x);
                    end.y = p.y -0.5f * ss.y - ( -0.5f * imgs.y + p.y); 
                    //end.x += deltax;
                }

                if(Mathf.Abs(p.y)< (Mathf.Abs(0.5f * imgs.y) - Mathf.Abs(0.5f * ss.y)))
                {
                    end.y = p.y;
                }
            }
            


            returner = StartCoroutine(Recenter(p, end));
        }

        IEnumerator Recenter(Vector3 start, Vector3 end)
        {
            returning = true;
            // constant return speed
            float d = Vector3.Distance(end,start);
            returnTime = d/returnSpeed;
            
            float t = 0f;
            while(t<returnTime)
            {
                t += Time.deltaTime;

                target.transform.position = Vector3.Lerp(start, end, t * returnSpeed);
                yield return null;
            }
            
            returning = false;
            //Debug.Log("returning to center");
        }


        private void LateUpdate()
        {

            int touchCount = Input.touchCount;

            if (FingersScript.Instance.TreatMousePointerAsFinger && Input.mousePresent)
            {
                touchCount += (Input.GetMouseButton(0) ? 1 : 0);
                touchCount += (Input.GetMouseButton(1) ? 1 : 0);
                touchCount += (Input.GetMouseButton(2) ? 1 : 0);
            }
            string touchIds = string.Empty;
            int gestureTouchCount = 0;
            foreach (GestureRecognizer g in FingersScript.Instance.Gestures)
            {
                gestureTouchCount += g.CurrentTrackedTouches.Count;
            }
            foreach (GestureTouch t in FingersScript.Instance.CurrentTouches)
            {
                touchIds += ":" + t.Id + ":";
            }
        }
    }
}


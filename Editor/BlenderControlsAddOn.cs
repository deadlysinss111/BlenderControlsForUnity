using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

//[InitializeOnLoad]
public class BlenderControlsAddOn : SceneView
{
    [MenuItem("Window/SceneWithBlenderConstrols")]
    private static void ShowWindow()
    {
        GetWindow<BlenderControlsAddOn>().Show();
    }

    static BlenderControlsAddOn()
    {
        //EditorApplication.update += UpdateMethod;
    }

    //static void UpdateMethod()
    //{
    //    if (Input.anyKey)
    //    {
    //        Debug.Log("A key was pressed!");
    //    }
    //}

    enum Action
    {
        None = 0,
        Move,
        Rotate,
        Scale,
    }

    enum AngeLock
    {
        None = 0,
        X = 1,
        Y = 2,
        Z = 4,
    }

    Action _action;
    int _angleLocked;
    private static Transform _frameStartTransform;

    private static Vector3 _selectionStartPos;
    private static Vector3 _selectionStartScale;
    private static Quaternion _selectionStartRot;

    protected override void OnSceneGUI()
    {
        base.OnSceneGUI();

        if (Selection.activeGameObject == null) return;

        Event e = Event.current;

        switch (e.type)
        {
            case EventType.KeyDown:

            switch (e.keyCode)
            {
                case KeyCode.G:
                    if(_action != Action.Move)
                    {
                        _action = Action.Move;

                        _selectionStartPos = Selection.activeGameObject.transform.position;
                        _selectionStartScale = Selection.activeGameObject.transform.localScale;
                        _selectionStartRot = Selection.activeGameObject.transform.rotation;

                        _frameStartTransform = Selection.activeGameObject.transform;
                    }
                    else
                    {
                        _action = Action.None;
                        _angleLocked = 0;
                    }
                    break;
                case KeyCode.R:
                    if (_action != Action.Rotate)
                    {
                        _action = Action.Rotate;

                        _selectionStartPos = Selection.activeGameObject.transform.position;
                        _selectionStartScale = Selection.activeGameObject.transform.localScale;
                        _selectionStartRot = Selection.activeGameObject.transform.rotation;

                        _frameStartTransform = Selection.activeGameObject.transform;
                    }
                    else
                    {
                        _action = Action.None;
                        _angleLocked = 0;
                    }
                    break;
                case KeyCode.S:
                    if (_action != Action.Scale)
                    {
                        _action = Action.Scale;

                        _selectionStartPos = Selection.activeGameObject.transform.position;
                        _selectionStartScale = Selection.activeGameObject.transform.localScale;
                        _selectionStartRot = Selection.activeGameObject.transform.rotation;

                        _frameStartTransform = Selection.activeGameObject.transform;
                    }
                    else
                    {
                        _action = Action.None;
                        _angleLocked = 0;
                    }
                    break;
                case KeyCode.X:
                    if ((_angleLocked & 1) == 0)
                    {
                        if ((e.modifiers & EventModifiers.Shift) != 0)
                            _angleLocked = 6;
                        else
                            _angleLocked = 1;
                    }
                    else
                    {
                        _angleLocked = 0;
                    }
                    break;
                case KeyCode.Y:
                    if ((_angleLocked & 2) == 0)
                    {
                        if ((e.modifiers & EventModifiers.Shift) != 0)
                            _angleLocked = 5;
                        else
                            _angleLocked = 2;
                    }
                    else
                    {
                        _angleLocked = 0;
                    }
                    break;
                case KeyCode.Z:
                    if ((_angleLocked & 4) == 0)
                    {
                        if ((e.modifiers & EventModifiers.Shift) != 0)
                            _angleLocked = 3;
                        else
                            _angleLocked += 4;
                    }
                    else
                    {
                        _angleLocked -= 4;
                    }
                    break;
                }
            break;
                

            case EventType.MouseMove:
                Transform selectedTransform = Selection.activeGameObject.transform;
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                switch (_action)
                {
                    case Action.Move:
                        {
                            Vector3 axis = Vector3.zero;
                            int frameLock = _angleLocked;
                            Plane moveAlong;

                            if (frameLock == 0)
                            {
                                moveAlong = new Plane(SceneView.lastActiveSceneView.camera.transform.forward, _frameStartTransform.position);
                            }
                            else
                            {
                                if (frameLock / 4 > 0)
                                {
                                    axis.z = 1;
                                    frameLock -= 4;
                                }
                                if (frameLock / 2 > 0)
                                {
                                    axis.y = 1;
                                    frameLock -= 2;
                                }
                                if (frameLock / 1 > 0)
                                {
                                    axis.x = 1;
                                }

                                if(axis.y == 0)
                                    moveAlong = new Plane(Vector3.up, _frameStartTransform.position);
                                else
                                {
                                    if(axis.x == 0)
                                        moveAlong = new Plane(Vector3.right, _frameStartTransform.position);
                                    else
                                        moveAlong = new Plane(Vector3.forward, _frameStartTransform.position);
                                }
                            }
                            if (moveAlong.Raycast(ray, out float positionDelta))
                            {
                                Vector3 dist = ray.GetPoint(positionDelta) - _frameStartTransform.position;
                                if(_angleLocked != 0)
                                {
                                    dist.x *= axis.x;
                                    dist.y *= axis.y;
                                    dist.z *= axis.z;
                                }

                                selectedTransform.position = dist + _frameStartTransform.position;
                            }
                        }
                        break;

                    case Action.Rotate:
                        if (new Plane(Vector3.up, _frameStartTransform.position).Raycast(ray, out float rotationDelta))
                        {
                            Vector3 axis = Vector3.zero;

                            int sign = 0;
                            if (Mathf.Abs(e.delta.x) > Mathf.Abs(e.delta.y))
                                sign = e.delta.x > 0 ? -1 : 1;
                            else
                                sign = e.delta.y > 0 ? 1 : -1;

                            if (_angleLocked == 0) 
                                axis = SceneView.lastActiveSceneView.camera.transform.up;
                            else if (_angleLocked == 1)
                                axis = Vector3.right;
                            else if (_angleLocked == 2)
                                axis = Vector3.up;
                            else if (_angleLocked == 4)
                            {
                                sign *= -1;
                                axis = Vector3.forward;
                            } 

                            

                            selectedTransform.rotation = _frameStartTransform.rotation * Quaternion.AngleAxis(rotationDelta * .1f * sign, axis);
                        }
                        break;

                    case Action.Scale:
                        if (new Plane(Vector3.up, _frameStartTransform.position).Raycast(ray, out float scaleDelta))
                        {
                            Vector3 axis = Vector3.zero;
                            int frameLock = _angleLocked;

                            if (frameLock == 0)
                            {
                                axis = Vector3.one;
                            }
                            else
                            {
                                if (frameLock % 4 > 0)
                                {
                                    axis.z = 1;
                                    frameLock -= 4;
                                }
                                if (frameLock % 2 > 0)
                                {
                                    axis.y = 1;
                                    frameLock -= 2;
                                }
                                if (frameLock % 1 > 0)
                                {
                                    axis.x = 1;
                                }
                            }

                            int sign = 0;
                            if(Mathf.Abs(e.delta.x) > Mathf.Abs(e.delta.y))
                                sign = e.delta.x > 0 ? 1 : -1;
                            else
                                sign = e.delta.y > 0 ? -1 : 1;

                            Vector3 scaleChange = Vector3.one + (axis * scaleDelta * .002f) * sign;
                            Vector3 newScale = Vector3.Max(Vector3.Scale(_frameStartTransform.localScale, scaleChange), Vector3.one * 0.01f);

                            selectedTransform.localScale = newScale;
                        }
                        break;
                }
                break;

            case EventType.MouseDown:
                _action = Action.None;
                _angleLocked = 0;
                if(e.button == (int)MouseButton.RightMouse)
                {
                    Selection.activeGameObject.transform.position = _selectionStartPos;
                    Selection.activeGameObject.transform.localScale = _selectionStartScale;
                    Selection.activeGameObject.transform.rotation = _selectionStartRot;
                }
                else if(e.button == (int)MouseButton.LeftMouse)
                {
                    _selectionStartPos = Selection.activeGameObject.transform.position;
                    _selectionStartScale = Selection.activeGameObject.transform.localScale;
                    _selectionStartRot = Selection.activeGameObject.transform.rotation;
                }
                break;
        }
        Tools.current = Tool.None;
    }
}

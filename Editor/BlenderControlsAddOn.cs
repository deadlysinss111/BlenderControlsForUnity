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

    private static GameObject _previousSelection = null;

    protected override void OnSceneGUI()
    {
        base.OnSceneGUI();

        var currentSelection = Selection.activeGameObject;

        if (currentSelection == null)
        {
            _previousSelection = null;
            return;
        }

        Event e = Event.current;

        if ((e.modifiers & EventModifiers.Control) != 0 || (e.modifiers & EventModifiers.Command) != 0) return;

        if(_previousSelection != currentSelection)
        {
            _previousSelection = currentSelection;
            Debug.Log("changed target");
            
        }

        switch (e.type)
        {
            case EventType.KeyDown:

            switch (e.keyCode)
            {
                case KeyCode.G:
                    if(_action != Action.Move)
                    {
                        _action = Action.Move;

                        _selectionStartPos = currentSelection.transform.position;
                        _selectionStartScale = currentSelection.transform.localScale;
                        _selectionStartRot = currentSelection.transform.rotation;

                        _frameStartTransform = currentSelection.transform;
                    }
                    else
                    {
                        _action = Action.None;
                        _angleLocked = 0;
                        ApplyUndoRecordPosition(currentSelection.transform);
                    }
                    break;
                case KeyCode.R:
                    if (_action != Action.Rotate)
                    {
                        _action = Action.Rotate;

                        _selectionStartPos = currentSelection.transform.position;
                        _selectionStartScale = currentSelection.transform.localScale;
                        _selectionStartRot = currentSelection.transform.rotation;

                        _frameStartTransform = currentSelection.transform;
                    }
                    else
                    {
                        _action = Action.None;
                        _angleLocked = 0;
                         ApplyUndoRecordRotation(currentSelection.transform);
                    }
                    break;
                case KeyCode.S:
                    if (_action != Action.Scale)
                    {
                        _action = Action.Scale;

                        _selectionStartPos = currentSelection.transform.position;
                        _selectionStartScale = currentSelection.transform.localScale;
                        _selectionStartRot = currentSelection.transform.rotation;

                        _frameStartTransform = currentSelection.transform;
                    }
                    else
                    {
                        _action = Action.None;
                        _angleLocked = 0;
                        ApplyUndoRecordScale(currentSelection.transform);
                    }
                    break;
                case KeyCode.X:
                    if ((_angleLocked & 1) == 0)
                    {
                        if ((e.modifiers & EventModifiers.Shift) != 0)
                        {
                            ResetValues(currentSelection.transform, AngeLock.X);
                            _angleLocked = 6;
                        }
                        else
                        {
                            ResetValues(currentSelection.transform, AngeLock.Y | AngeLock.Z);
                            _angleLocked = 1;
                        }
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
                        {
                            ResetValues(currentSelection.transform, AngeLock.Y);
                            _angleLocked = 5;
                        }
                        else
                        {
                            ResetValues(currentSelection.transform, AngeLock.X | AngeLock.Z);
                            _angleLocked = 2;
                        }
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
                        {
                            ResetValues(currentSelection.transform, AngeLock.Z);
                            _angleLocked = 3;
                        }
                        else
                        {
                            ResetValues(currentSelection.transform, AngeLock.X | AngeLock.Y);
                            _angleLocked = 4;
                        }
                    }
                    else
                    {
                        _angleLocked = 0;
                    }
                    break;
                }
            break;
                

            case EventType.MouseMove:
                Transform selectedTransform = currentSelection.transform;
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
                if(e.button == (int)MouseButton.RightMouse && _action != Action.None)
                {
                    currentSelection.transform.position = _selectionStartPos;
                    currentSelection.transform.localScale = _selectionStartScale;
                    currentSelection.transform.rotation = _selectionStartRot;

                    _action = Action.None;
                    _angleLocked = 0;
                }
                else if(e.button == (int)MouseButton.LeftMouse && _action != Action.None)
                {
                    switch (_action)
                    {
                        case Action.Move:
                            ApplyUndoRecordPosition(currentSelection.transform);
                            break;
                        case Action.Rotate:
                            ApplyUndoRecordRotation(currentSelection.transform);
                            break;
                        case Action.Scale:
                            ApplyUndoRecordScale(currentSelection.transform);
                            break;
                        default:
                            Debug.LogError("Tried to validate an action that is set to none");
                            break;
                    }
                    _action = Action.None;
                    _angleLocked = 0;
                }
                break;
        }
        Tools.current = Tool.None;
    }

    void ApplyUndoRecordPosition(Transform obj)
    {
        Vector3 pos = obj.position;
        obj.position = _selectionStartPos;
        Undo.RecordObject(obj, "Move Object");
        obj.position = pos;
    }

    void ApplyUndoRecordRotation(Transform obj)
    {
        Quaternion rot = obj.rotation;
        obj.rotation = _selectionStartRot;
        Undo.RecordObject(obj, "Rotate Object");
        obj.rotation = rot;
    }

    void ApplyUndoRecordScale(Transform obj)
    {
        Vector3 scale = obj.localScale;
        obj.localScale = _selectionStartScale;
        Undo.RecordObject(obj, "Scale Object");
        obj.localScale = scale;
    }

    void ResetValues(Transform obj, AngeLock axisAsEnum)
    {
        int axis = (int)axisAsEnum;

        // Z
        if (axis / 4 > 0)
        {
            axis -= 4;

            //position
            Vector3 pos = obj.position;
            pos.z = _selectionStartPos.z;
            obj.position = pos;

            //rotation
            Vector3 euler = obj.rotation.eulerAngles;
            euler.z = _selectionStartRot.eulerAngles.z;
            obj.rotation = Quaternion.Euler(euler);

            //scale
            Vector3 scale = obj.localScale;
            scale.z = _selectionStartScale.z;
            obj.localScale = scale;
        }
        // Y
        if (axis / 2 > 0)
        {
            axis -= 2;

            //position
            Vector3 pos = obj.position;
            pos.y = _selectionStartPos.y;
            obj.position = pos;

            //rotation
            Vector3 euler = obj.rotation.eulerAngles;
            euler.y = _selectionStartRot.eulerAngles.y;
            obj.rotation = Quaternion.Euler(euler);

            //scale
            Vector3 scale = obj.localScale;
            scale.y = _selectionStartScale.y;
            obj.localScale = scale;
        }
        // X
        if (axis / 1 > 0)
        {
            //position
            Vector3 pos = obj.position;
            pos.x = _selectionStartPos.x;
            obj.position = pos;

            //rotation
            Vector3 euler = obj.rotation.eulerAngles;
            euler.x = _selectionStartRot.eulerAngles.x;
            obj.rotation = Quaternion.Euler(euler);

            //scale
            Vector3 scale = obj.localScale;
            scale.x = _selectionStartScale.x;
            obj.localScale = scale;
        }
    }
}

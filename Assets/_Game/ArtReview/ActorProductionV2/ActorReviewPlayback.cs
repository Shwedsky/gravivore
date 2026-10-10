using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gravivore.ArtReview
{
    /// <summary>Isolated review controls only. No combat, collision or progression authority.</summary>
    public sealed class ActorReviewPlayback : MonoBehaviour
    {
        [SerializeField] private GameObject[] _actors = Array.Empty<GameObject>();
        [SerializeField] private GameObject _playerReference;
        [SerializeField] private Camera _camera;
        [SerializeField] private Vector3 _cameraOffset;
        [SerializeField] private float _lookAtHeight;
        private Animator[] _animators;
        private LODGroup[] _lods;
        private string[][] _states;
        private int _selected, _pose, _lod = -1;
        private bool _overview;
        private static readonly string[] CandidateStates = { "Idle", "Run", "Attack", "Hit", "Death", "Block", "Cut", "Hover", "Discharge", "Bank", "Charge", "Vent", "Telegraph", "AttackLine", "AttackCircle", "AttackCone" };
        public int ActorCount => _actors.Length;
        public int SelectedIndex => _selected;
        public int ReviewPoseCount => _states[_selected].Length;
        public void Configure(GameObject[] actors, GameObject player, Camera camera, Vector3 offset, float lookAt)
        { _actors = actors; _playerReference = player; _camera = camera; _cameraOffset = offset; _lookAtHeight = lookAt; }
        private void Awake()
        {
            _animators = new Animator[_actors.Length]; _lods = new LODGroup[_actors.Length]; _states = new string[_actors.Length][];
            for (var i = 0; i < _actors.Length; i++)
            {
                _animators[i] = _actors[i].GetComponentInChildren<Animator>(true);
                _lods[i] = _actors[i].GetComponent<LODGroup>();
                _animators[i].applyRootMotion = false;
                _actors[i].SetActive(true); _animators[i].Rebind(); _animators[i].Update(0);
                var available = new List<string>();
                foreach (var state in CandidateStates) if (_animators[i].HasState(0, Animator.StringToHash(state))) available.Add(state);
                if (available.Count == 0) throw new InvalidOperationException(_actors[i].name + " has no review poses");
                _states[i] = available.ToArray();
            }
            ShowActor(0);
        }
        public void ShowActor(int index)
        {
            if (index < 0 || index >= _actors.Length) throw new ArgumentOutOfRangeException(nameof(index));
            _selected = index; _overview = false;
            for (var i = 0; i < _actors.Length; i++)
            {
                _actors[i].SetActive(i == index); _actors[i].transform.position = Vector3.zero;
            }
            _playerReference.transform.position = new Vector3(-2.25f, 0, 0);
            _camera.transform.position = _cameraOffset; _camera.transform.LookAt(Vector3.up * _lookAtHeight);
            PlayPose(_pose);
        }
        public void PlayPose(int index)
        {
            _pose = index % _states[_selected].Length; var animator = _animators[_selected];
            var state = Animator.StringToHash(_states[_selected][_pose]);
            animator.Rebind(); animator.Play(state, 0, 0); animator.Update(0);
        }
        public void SetLod(int index)
        {
            if (index < -1 || index > 2) throw new ArgumentOutOfRangeException(nameof(index));
            _lod = index; foreach (var group in _lods) group.ForceLOD(index);
        }
        public void ShowFamily()
        {
            _overview = true;
            for (var i = 0; i < _actors.Length; i++)
            {
                _actors[i].SetActive(true);
                _actors[i].transform.position = i < 5 ? new Vector3((i - 2) * 2.4f, 0, -1.7f) : new Vector3((i - 5.5f) * 5, 0, 3);
                _animators[i].Rebind(); _animators[i].Play("Idle", 0, 0);
            }
            _playerReference.transform.position = new Vector3(-5.8f, 0, 3);
            _camera.transform.position = _cameraOffset * 1.6f; _camera.transform.LookAt(Vector3.up * _lookAtHeight);
        }
        private void OnGUI()
        {
            // Review-only diagnostic UI. It is not part of the gameplay UI or a
            // zero-allocation performance measurement. No runtime subscriptions.
            var scale = Mathf.Max(1, Screen.width / 540f);
            var previous = GUI.matrix; GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);
            var width = Screen.width / scale;
            GUI.Box(new Rect(8, 8, width - 16, 54), "GRAVIVORE / ACTOR PRODUCTION REVIEW\n" + (_overview ? "Complete family / actual model scale" : _actors[_selected].name + " / " + _states[_selected][_pose]));
            var y = Screen.height / scale - 92;
            if (GUI.Button(new Rect(8, y, width / 3 - 12, 38), "Next actor")) ShowActor((_selected + 1) % _actors.Length);
            if (GUI.Button(new Rect(width / 3 + 4, y, width / 3 - 12, 38), "Next pose")) { if (_overview) ShowActor(_selected); PlayPose(_pose + 1); }
            if (GUI.Button(new Rect(width * 2 / 3, y, width / 3 - 8, 38), "LOD " + (_lod < 0 ? "Auto" : _lod.ToString()))) SetLod(_lod == 2 ? -1 : _lod + 1);
            if (GUI.Button(new Rect(8, y + 42, width - 16, 38), "Complete family")) ShowFamily();
            GUI.matrix = previous;
        }
    }
}

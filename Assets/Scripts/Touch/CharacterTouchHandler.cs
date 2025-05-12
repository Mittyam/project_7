using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Live2D.Cubism.Framework.Raycasting;
using UnityEngine.EventSystems;

/// <summary>
/// Live2D�L�����N�^�[�̃^�b�`�@�\���Ǘ�����R���|�[�l���g
/// ���C���X�e�[�g�ł̂ݓ��I�ɃA�^�b�`����A�m�x���X�e�[�g�ł͎g�p����Ȃ�
/// </summary>
public class CharacterTouchHandler : MonoBehaviour
{
    private CubismRaycaster raycaster;
    private List<CubismRaycastable> raycastables = new List<CubismRaycastable>();
    private Live2DController live2DController;
    private string currentModelID;

    // ���f���ƃA�^�b�`��Ԃ̃f�o�b�O�p
    private bool isInitialized = false;
    private bool hasRaycastablesSetup = false;

    // �����蔻��̃}�b�s���O��`
    [System.Serializable]
    public class DrawableMapping
    {
        public string drawableId;  // ���f����Drawable ID�i��F�uHead�v�uBody�v�Ȃǁj
        public TouchArea touchArea;  // �Ή�����^�b�`�̈�
    }

    [SerializeField] private List<DrawableMapping> drawableMappings = new List<DrawableMapping>();

    [System.Serializable]
    public class TouchReaction
    {
        public TouchArea area;
        public string[] animationTriggers;
    }

    [SerializeField] private List<TouchReaction> touchReactions = new List<TouchReaction>();

    // �f�t�H���g�̃��A�N�V�����ݒ�
    private void SetDefaultReactions()
    {
        if (touchReactions.Count == 0)
        {
            // �����^�b�`�p���A�N�V����
            touchReactions.Add(new TouchReaction
            {
                area = TouchArea.Head,
                animationTriggers = new string[] { "Anim_1", "Anim_3" }
            });

            // ���̃^�b�`�p���A�N�V����
            touchReactions.Add(new TouchReaction
            {
                area = TouchArea.Chest,
                animationTriggers = new string[] { "body_touch_1", "body_touch_2" }
            });

            // ����G���A�^�b�`�p���A�N�V����
            touchReactions.Add(new TouchReaction
            {
                area = TouchArea.Leg,
                animationTriggers = new string[] { "special_touch_1", "special_touch_2" }
            });
        }

        // �f�t�H���g��Drawable�}�b�s���O
        if (drawableMappings.Count == 0)
        {
            // ��ʓI��Live2D���f���̖����K���Ɋ�Â��}�b�s���O��
            // ���ۂ̃��f���ɍ��킹�ĕύX���K�v
            drawableMappings.Add(new DrawableMapping { drawableId = "Head", touchArea = TouchArea.Head });
            drawableMappings.Add(new DrawableMapping { drawableId = "Chest", touchArea = TouchArea.Chest });
            drawableMappings.Add(new DrawableMapping { drawableId = "Leg", touchArea = TouchArea.Leg });
        }
    }

    /// <summary>
    /// ���������� - ������
    /// </summary>
    public void Initialize(string modelID = "")
    {
        // �d����������h�~
        if (isInitialized)
        {
            Debug.Log($"CharacterTouchHandler already initialized for model: {currentModelID}");
            return;
        }

        currentModelID = modelID;

        // Live2D�R���g���[���[�̎Q�Ǝ擾
        live2DController = GetComponent<Live2DController>();
        if (live2DController == null)
        {
            live2DController = GetComponentInParent<Live2DController>();
        }

        // �f�t�H���g�̃��A�N�V�����ݒ�
        SetDefaultReactions();

        // CubismRaycaster�̎擾�܂��̓A�^�b�`
        raycaster = GetComponent<CubismRaycaster>();
        if (raycaster == null)
        {
            raycaster = gameObject.AddComponent<CubismRaycaster>();
            Debug.Log("Added CubismRaycaster to model");
        }

        // Drawables���������đΉ�����CubismRaycastable���A�^�b�`
        SetupRaycastables();

        // Update�֐��Ń^�b�`���͂����o����悤�ɂ���
        enabled = true;
        isInitialized = true;
    }

    /// <summary>
    /// Drawables���������ACubismRaycastable���Z�b�g�A�b�v
    /// </summary>
    private void SetupRaycastables()
    {
        // �d���Z�b�g�A�b�v��h�~
        if (hasRaycastablesSetup)
        {
            Debug.Log("Raycastables already set up - skipping");
            return;
        }

        // ���f����Drawables���擾
        var drawables = GetComponentsInChildren<Live2D.Cubism.Core.CubismDrawable>(true);
        if (drawables == null || drawables.Length == 0)
        {
            Debug.LogWarning($"No CubismDrawables found in model: {gameObject.name}");
            return;
        }

        // Debug.Log($"Found {drawables.Length} drawables in model {gameObject.name}");
        int setupCount = 0;

        foreach (var drawable in drawables)
        {
            // Drawable��ID���m�F
            string drawableId = drawable.name;

            // �}�b�s���O�Ώۂ��`�F�b�N
            var mapping = drawableMappings.FirstOrDefault(m => drawableId.Contains(m.drawableId));
            if (mapping != null)
            {
                // CubismRaycastable���Ȃ���Βǉ�
                var raycastable = drawable.gameObject.GetComponent<CubismRaycastable>();
                if (raycastable == null)
                {
                    raycastable = drawable.gameObject.AddComponent<CubismRaycastable>();
                    // ���x�ݒ�iTriangles�̕������m�������ׂ������j
                    raycastable.Precision = CubismRaycastablePrecision.Triangles;
                    setupCount++;
                }

                raycastables.Add(raycastable);
            }
        }

        // Debug.Log($"SetupRaycastables completed: {setupCount} new raycastables added");
        hasRaycastablesSetup = true;
    }

    /// <summary>
    /// ���t���[���̍X�V����
    /// </summary>
    private void Update()
    {
        // �}�E�X�N���b�N���Ȃ��ꍇ�͑������^�[��
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        // UI�̏�ŃN���b�N���ꂽ�ꍇ�͖�������
        if (IsPointerOverUI())
        {
            return;
        }

        // ���C���J��������}�E�X�ʒu�Ɍ����Ĕ�΂�
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // �����蔻�茋�ʂ��i�[����z��
        var results = new CubismRaycastHit[4];

        // ���C�L���X�g���s
        var hitCount = raycaster.Raycast(ray, results);

        if (hitCount > 0)
        {
            // ��������Drawable������
            for (var i = 0; i < hitCount; i++)
            {
                string drawableName = results[i].Drawable.name;
                Debug.Log($"Hit drawable: {drawableName}");

                // �}�b�s���O��������
                foreach (var mapping in drawableMappings)
                {
                    if (drawableName.Contains(mapping.drawableId))
                    {
                        // �Ή�����^�b�`�̈悪��������
                        OnAreaTouched(mapping.touchArea);
                        return; // �ŏ��Ɉ�v�������̂Ŕ���
                    }
                }
            }
        }
    }

    /// <summary>
    /// �^�b�`�@�\�̖�����
    /// </summary>
    public void DisableTouchDetection()
    {
        // Update�֐��𖳌������ă^�b�`���o���~
        enabled = false;
    }

    /// <summary>
    /// �|�C���^�[��UI�v�f�̏�ɂ��邩�ǂ����𔻒�
    /// </summary>
    private bool IsPointerOverUI()
    {
        // EventSystem�����݂��邩�m�F
        if (EventSystem.current == null)
            return false;

        // �|�C���^�[��UI�v�f�̏�ɂ��邩�`�F�b�N (�^�b�`�Ή���)
        if (Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }

        // �}�E�X�p�̃`�F�b�N
        return EventSystem.current.IsPointerOverGameObject();
    }

    /// <summary>
    /// �̈�^�b�`���̏���
    /// </summary>
    public void OnAreaTouched(TouchArea area)
    {
        Debug.Log($"Live2D�L�����N�^�[��{area}�G���A�ɐG��܂���");

        // �L���ȃ^�b�`�G���A�ɑ΂��锽��������
        var validReactions = touchReactions.Where(r => r.area == area).ToArray();

        if (validReactions.Length > 0 && live2DController != null)
        {
            // �����_���ɔ�����I��
            var reaction = validReactions[Random.Range(0, validReactions.Length)];

            if (reaction.animationTriggers != null && reaction.animationTriggers.Length > 0)
            {
                // �����_���ȃA�j���[�V�����g���K�[��I��
                string trigger = reaction.animationTriggers[
                    Random.Range(0, reaction.animationTriggers.Length)];

                // ���f��ID�̔���
                string modelID = !string.IsNullOrEmpty(currentModelID) ?
                                 currentModelID : live2DController.GetActiveModelID();

                if (!string.IsNullOrEmpty(modelID))
                {
                    // Live2D�R���g���[���[�ŃA�j���[�V�����Đ�
                    live2DController.PlayAnimation(modelID, trigger);
                    Debug.Log($"���f�� '{modelID}' �̃A�j���[�V���� '{trigger}' ���Đ����Ă��܂�");
                }
                else
                {
                    Debug.LogWarning("�A�N�e�B�u��Live2D���f����������܂���");
                }
            }
        }
    }

    /// <summary>
    /// �R���|�[�l���g�j�����̏���
    /// </summary>
    private void OnDestroy()
    {
        // �^�b�`�@�\�𖳌���
        DisableTouchDetection();

        // ��Ԃ����Z�b�g
        isInitialized = false;
        hasRaycastablesSetup = false;
        raycastables.Clear();
    }
}

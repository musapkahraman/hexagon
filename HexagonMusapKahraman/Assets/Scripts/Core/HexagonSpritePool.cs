using System.Collections.Generic;
using HexagonMusapKahraman.GridMap;
using HexagonMusapKahraman.UI;
using UnityEngine;

namespace HexagonMusapKahraman.Core
{
    public class HexagonSpritePool : MonoBehaviour
    {
        [SerializeField] private GameObject rotatingParentPrefab;
        [SerializeField] private GameObject hexagonSpritePrefab;
        [SerializeField] private GameObject hexagonSpriteMaskPrefab;
        [SerializeField] private CellSelector cellSelector;
        [SerializeField] private Grid grid;
        [SerializeField] private GridResizer gridResizer;
        [SerializeField] private BombTimerController bombTimerController;
        private readonly List<GameObject> _rotatingHexagonSpriteMasks = new List<GameObject>();
        private readonly List<HexagonRelation> _rotatingHexagonSprites = new List<HexagonRelation>();
        private readonly Queue<GameObject> _spriteMasks = new Queue<GameObject>();
        private readonly Queue<HexagonRelation> _sprites = new Queue<HexagonRelation>();
        private Transform _rotatingParent;

        private void Awake()
        {
            _rotatingParent = Instantiate(rotatingParentPrefab, transform.position, Quaternion.identity).transform;
            for (var i = 0; i < cellSelector.GetSelectionCount(); i++)
            {
                var hexagonSprite = BuildHexagonSprite();
                hexagonSprite.transform.parent = _rotatingParent;
                _rotatingHexagonSprites.Add(hexagonSprite);

                var hexagonSpriteMask = Instantiate(hexagonSpriteMaskPrefab, transform.position, Quaternion.identity);
                _rotatingHexagonSpriteMasks.Add(hexagonSpriteMask);
            }

            int cellCount = (int) gridResizer.GetGridSize().x * (int) gridResizer.GetGridSize().y;
            for (var i = 0; i < cellCount; i++)
            {
                var sprite = BuildHexagonSprite();
                sprite.DisableRenderer();
                _sprites.Enqueue(sprite);
                var mask = Instantiate(hexagonSpriteMaskPrefab, transform.position, Quaternion.identity);
                _spriteMasks.Enqueue(mask);
            }
        }

        private HexagonRelation BuildHexagonSprite()
        {
            var hexagonRelation = Instantiate(hexagonSpritePrefab, transform.position, Quaternion.identity)
                .GetComponent<HexagonRelation>();
            hexagonRelation.SetGrid(grid);
            return hexagonRelation;
        }

        public IEnumerable<HexagonRelation> GetRotatingHexagonSprites()
        {
            return _rotatingHexagonSprites;
        }

        public HexagonRelation GetHexagonSprite(PlacedHexagon placedHexagon)
        {
            var sprite = _sprites.Dequeue();
            sprite.transform.position = grid.GetCellCenterWorld(placedHexagon.Cell);
            sprite.SetRelation(placedHexagon);
            sprite.SetSprite(placedHexagon.Hexagon.tile.sprite);
            sprite.SetColor(placedHexagon.Hexagon.color);
            sprite.EnableRenderer();
            return sprite;
        }

        public void ReturnHexagonSprite(HexagonRelation hexagonRelation)
        {
            hexagonRelation.RemoveRelation();
            hexagonRelation.DisableRenderer();
            hexagonRelation.transform.position = transform.position;
            _sprites.Enqueue(hexagonRelation);
        }

        public Transform GetRotatingParent(Vector3 center, List<PlacedHexagon> neighbors)
        {
            _rotatingParent.position = center;
            if (neighbors.Count > _rotatingHexagonSprites.Count)
            {
                Debug.LogError("You should not change selection count while playing!");
                return _rotatingParent;
            }

            for (var i = 0; i < neighbors.Count; i++)
            {
                var placedHexagon = neighbors[i];
                var spritePosition = grid.GetCellCenterWorld(placedHexagon.Cell);
                _rotatingHexagonSpriteMasks[i].transform.position = spritePosition;
                var hexagonRelation = _rotatingHexagonSprites[i];
                hexagonRelation.SetRelation(placedHexagon);
                hexagonRelation.SetSprite(placedHexagon.Hexagon.tile.sprite);
                hexagonRelation.SetColor(placedHexagon.Hexagon.color);
                var spriteTransform = hexagonRelation.transform;
                spriteTransform.parent = null;
                spriteTransform.position = spritePosition;
                spriteTransform.parent = _rotatingParent;
                if (!(placedHexagon is BombHexagon bombHexagon)) continue;
                bombTimerController.Show(spriteTransform);
                bombTimerController.SetTimerText(bombHexagon.Timer);
            }

            return _rotatingParent;
        }

        public void ReturnRotatingParent()
        {
            for (var i = 0; i < _rotatingHexagonSprites.Count; i++)
            {
                _rotatingHexagonSprites[i].RemoveRelation();
                _rotatingHexagonSpriteMasks[i].transform.position = transform.position;
            }
            _rotatingParent.position = transform.position;
            _rotatingParent.rotation = Quaternion.identity;
        }

        public GameObject GetMask(Vector3 position)
        {
            var mask = _spriteMasks.Dequeue();
            mask.transform.position = position;
            return mask;
        }

        public void ReturnMask(GameObject mask)
        {
            mask.transform.position = transform.position;
            _spriteMasks.Enqueue(mask);
        }
    }
}
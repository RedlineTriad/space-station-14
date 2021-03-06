﻿using Content.Client.GameObjects.EntitySystems;
using Content.Shared.GameObjects.Components;
using Robust.Client.GameObjects;
using Robust.Client.Interfaces.GameObjects.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.GameObjects.Components.Transform;
using Robust.Shared.Serialization;
using static Content.Client.GameObjects.Components.IconSmoothing.IconSmoothComponent;

namespace Content.Client.GameObjects.Components
{
    [RegisterComponent]
    [ComponentReference(typeof(SharedWindowComponent))]
    public sealed class WindowComponent : SharedWindowComponent
    {
        private string _stateBase;
        private ISpriteComponent _sprite;
        private SnapGridComponent _snapGrid;

        public override void Initialize()
        {
            base.Initialize();

            _sprite = Owner.GetComponent<ISpriteComponent>();
            _snapGrid = Owner.GetComponent<SnapGridComponent>();
        }

        /// <inheritdoc />
        protected override void Startup()
        {
            base.Startup();

            _snapGrid.OnPositionChanged += SnapGridOnPositionChanged;
            Owner.EntityManager.EventBus.RaiseEvent(EventSource.Local, new WindowSmoothDirtyEvent(Owner));

            var state0 = $"{_stateBase}0";
            _sprite.LayerMapSet(CornerLayers.SE, _sprite.AddLayerState(state0));
            _sprite.LayerSetDirOffset(CornerLayers.SE, SpriteComponent.DirectionOffset.None);
            _sprite.LayerMapSet(CornerLayers.NE, _sprite.AddLayerState(state0));
            _sprite.LayerSetDirOffset(CornerLayers.NE, SpriteComponent.DirectionOffset.CounterClockwise);
            _sprite.LayerMapSet(CornerLayers.NW, _sprite.AddLayerState(state0));
            _sprite.LayerSetDirOffset(CornerLayers.NW, SpriteComponent.DirectionOffset.Flip);
            _sprite.LayerMapSet(CornerLayers.SW, _sprite.AddLayerState(state0));
            _sprite.LayerSetDirOffset(CornerLayers.SW, SpriteComponent.DirectionOffset.Clockwise);
        }

        /// <inheritdoc />
        protected override void Shutdown()
        {
            _snapGrid.OnPositionChanged -= SnapGridOnPositionChanged;

            base.Shutdown();
        }

        private void SnapGridOnPositionChanged()
        {
            Owner.EntityManager.EventBus.RaiseEvent(EventSource.Local, new WindowSmoothDirtyEvent(Owner));
        }

        public void UpdateSprite()
        {
            var lowWall = FindLowWall();
            if (lowWall == null)
            {
                return;
            }

            _sprite.LayerSetState(CornerLayers.NE, $"{_stateBase}{(int) lowWall.LastCornerNE}");
            _sprite.LayerSetState(CornerLayers.SE, $"{_stateBase}{(int) lowWall.LastCornerSE}");
            _sprite.LayerSetState(CornerLayers.SW, $"{_stateBase}{(int) lowWall.LastCornerSW}");
            _sprite.LayerSetState(CornerLayers.NW, $"{_stateBase}{(int) lowWall.LastCornerNW}");
        }

        private LowWallComponent FindLowWall()
        {
            foreach (var entity in _snapGrid.GetLocal())
            {
                if (entity.TryGetComponent(out LowWallComponent lowWall))
                {
                    return lowWall;
                }
            }

            return null;
        }

        public override void ExposeData(ObjectSerializer serializer)
        {
            base.ExposeData(serializer);

            serializer.DataField(ref _stateBase, "base", null);
        }
    }
}

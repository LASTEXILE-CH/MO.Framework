﻿using MO.Unity3d.Data;
using MO.Unity3d.UIExtension;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace MO.Unity3d.Entities
{
    public class PlayerEntity : EntityLogic
    {
		private bool _isSelf;
		private Vector3 _offset;
		private float _positionSpeed = 2.0f;
		private float _rotateSpeed = 8.0f;
		private float currY = 0f;
		private PlayerData _playerData;

		protected internal override void OnInit(object userData)
		{
			base.OnInit(userData);
			//UIJoystickControl.Instance.joystickDragDelegate = OnJoystickDrag;
			_playerData = (PlayerData)userData;
			_isSelf = _playerData.UserId == GameUser.Instance.UserId;

			transform.position = _playerData.Position;
			transform.eulerAngles = _playerData.Rotate;

			if (_isSelf)
			{
				GetComponent<Renderer>().material.color = Color.blue;
				_offset = Camera.main.transform.position;
				var position = transform.position + _offset;
				Camera.main.transform.position = position;
			}
			else
			{
				GetComponent<Renderer>().material.color = Color.green;
			}
			_playerData.ShowHP();
		}

		protected internal override void OnHide(bool isShutdown, object userData)
		{
			base.OnHide(isShutdown, userData);
			//GameEntry.Entity.HideEntity(_playerData.HPEntityId);
		}

        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
		{
			//if (GameUser.Instance.JumpState)
			//{
			//	transform.position += transform.forward * _positionSpeed * 2;
			//	GameUser.Instance.IsJump = false;
			//}

			if (_playerData.JumpState > 0)
			{
				transform.position += transform.forward * Time.deltaTime * _positionSpeed * 4;

				if (_playerData.JumpState == 1)
				{
					currY += 0.1f;

					if (currY > 2.0f)
						_playerData.JumpState = 2;
				}
				else
				{
					currY -= 0.1f;
					if (currY < 0.0f)
					{
						_playerData.JumpState = 0;
						currY = 0.0f;
					}
				}

				Vector3 pos = transform.position;
				pos.y = currY;
				transform.position = pos;
			}

			if (_isSelf)
			{
				var eulerAngles = JoystickControl.GetDestination();
				if (eulerAngles != new Vector3())
				{
					Vector3 destDirection = new Vector3(eulerAngles.x, 0, eulerAngles.y);
					Quaternion quaternion = Quaternion.LookRotation(destDirection);
					transform.rotation = quaternion;
					transform.position += transform.forward * Time.deltaTime * _positionSpeed;
				}
				var position = new Vector3(transform.position.x, 0, transform.position.z) + _offset;
				Camera.main.transform.position = position;
			}
			else
            {
				var destDirection = _playerData.Rotate;
				if (Vector3.Distance(transform.eulerAngles, _playerData.Rotate) > 0.0004f)
				{
					transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(destDirection), _rotateSpeed * Time.deltaTime);
				}

				float distance = 0.0f;
				float deltaSpeed = (_positionSpeed * Time.deltaTime);

				var destPosition = _playerData.Position;
				distance = Vector3.Distance(destPosition, transform.position);

				if (distance > 0.01f)
				{
					Vector3 pos = transform.position;

					Vector3 movement = destPosition - pos;
					movement.y = 0f;
					movement.Normalize();

					movement *= deltaSpeed;

					if (distance > deltaSpeed || movement.magnitude > deltaSpeed)
						pos += movement;
					else
						pos = destPosition;

					transform.position = pos;
				}
			}
			FixedState();
			base.OnUpdate(elapseSeconds, realElapseSeconds);
		}

		private void FixedState()
		{
			if (_playerData.ResetState == 1)
			{
				transform.position = new Vector3();
				transform.eulerAngles = new Vector3();
				_playerData.ResetState = 2;
			}
			else
			{
				//500ms误差修正玩家位置
				var destDirection = _playerData.Rotate;
				if (Vector3.Distance(transform.eulerAngles, destDirection) > _rotateSpeed / 2)
				{
					transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(destDirection), _rotateSpeed * Time.deltaTime);
				}

				float distance = 0.0f;
				float deltaSpeed = (_positionSpeed * Time.deltaTime);

				var destPosition = _playerData.Position;
				distance = Vector3.Distance(destPosition, transform.position);

				if (distance > _positionSpeed / 2)
				{
					Vector3 pos = transform.position;

					Vector3 movement = destPosition - pos;
					movement.y = 0f;
					movement.Normalize();

					movement *= deltaSpeed;

					if (distance > deltaSpeed || movement.magnitude > deltaSpeed)
						pos += movement;
					else
						pos = destPosition;

					transform.position = pos;
				}
			}
		}
	}
}

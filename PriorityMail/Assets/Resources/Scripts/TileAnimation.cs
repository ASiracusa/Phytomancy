using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileAnimation
{
    protected TileElement tileElement;

    public abstract void Animate();

    public abstract void Complete();
}

public class TileAnimationMovement : TileAnimation
{
    private Vector3 targetPos;

    public TileAnimationMovement(TileElement _tileElement, Vector3 _targetPos)
    {
        tileElement = _tileElement;
        targetPos = _targetPos;
    }

    public override void Animate()
    {
        if (tileElement.model != null)
        {
            tileElement.model.transform.position = Vector3.Lerp(tileElement.model.transform.position, targetPos, 0.4f);
        }
    }

    public override void Complete()
    {
        if (tileElement.model != null)
        {
            tileElement.model.transform.position = targetPos;
        }
    }
}

public class TileAnimationFall : TileAnimation
{
    private Vector3 startPos;
    private Vector3 targetPos;

    public TileAnimationFall(TileElement _tileElement, Vector3 _targetPos)
    {
        tileElement = _tileElement;
        targetPos = _targetPos;
    }

    public override void Animate()
    {
        if (tileElement.model.transform.position.Equals(targetPos))
        {
            return;
        }
        tileElement.model.transform.position = new Vector3(startPos.x, startPos.y - 2.0f * (startPos.y - tileElement.model.transform.position.y) - 0.01f, startPos.z);

        if (tileElement.model.transform.position.y < targetPos.y)
        {
            tileElement.model.transform.position = targetPos;
        }
        else
        {
            tileElement.model.transform.localScale = new Vector3(6.0f / (startPos.y - tileElement.model.transform.position.y + 6), 3.0f - 2.0f / (startPos.y - tileElement.model.transform.position.y + 1), 6.0f / (startPos.y - tileElement.model.transform.position.y + 6));
        }
    }

    public override void Complete()
    {
        tileElement.model.transform.position = targetPos;
        tileElement.model.transform.localScale = Vector3.one;
    }

    public void SetStartPos()
    {
        startPos = tileElement.model.transform.position;
    }
}
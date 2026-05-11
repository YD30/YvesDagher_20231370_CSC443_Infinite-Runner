using System.Collections.Generic;
using UnityEngine;

public class CoinSystem : MonoBehaviour
{
    public static CoinSystem Instance;

    private readonly List<Coin> _activeCoins = new();

    void Awake()
    {
        Instance = this;
    }

    public void Register(Coin coin)
    {
        if (!_activeCoins.Contains(coin))
            _activeCoins.Add(coin);
    }

    public void ReturnCoin(Coin coin)
    {
        coin.gameObject.SetActive(false);
        _activeCoins.Remove(coin);
    }
}
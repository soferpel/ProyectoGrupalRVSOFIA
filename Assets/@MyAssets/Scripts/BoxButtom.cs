using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxButtom : MonoBehaviour
{
    public InfoUI infoUI;
    public ShelfBoxSpawner[] shelfBoxSpawners;
    public OrderController orderController;
    public int boxCost = 5;
    public void BuyBox()
    {
        if (orderController.cash >= boxCost)
        {
            orderController.cash -= boxCost;
            ShelfBoxSpawner shelf = null;
            int maxFreePoint = 0;
            foreach(ShelfBoxSpawner shelfBox in shelfBoxSpawners)
            {
                int freePoints = shelfBox.GetFreePoints();
                if (freePoints == 9)
                {
                    shelfBox.SpawnBoxes();
                    return;
                }else if (maxFreePoint<freePoints){
                    maxFreePoint = freePoints;
                    shelf = shelfBox;
                }
            }
            if(maxFreePoint>0)
            {
                shelf.SpawnBoxes();
            }
        }
    }
}
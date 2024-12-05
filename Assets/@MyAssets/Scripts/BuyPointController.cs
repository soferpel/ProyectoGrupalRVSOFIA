using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuyPointController : MonoBehaviour
{
    public Transform buyPointTransform;
    public List<ClientController> customerQueue = new List<ClientController>();
    public int maxQueueSize = 5;
    public ClientController currentCustomer = null;

    public bool IsOccupied()
    {
        return currentCustomer != null;
    }

    public bool PlaceInQueue(ClientController customer)
    {
        if (customerQueue.Count < maxQueueSize)
        {
            customerQueue.Add(customer);
            customer.inQueue = true;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void OccupyPoint(ClientController customer)
    {
        currentCustomer = customer;
    }

    public void FreePoint()
    {
        if (currentCustomer != null)
        {
            UpdateQueuePositions();
            if (customerQueue.Count > 0)
            {
                ClientController nextCustomer = customerQueue[0];
                customerQueue.RemoveAt(0);
                OccupyPoint(nextCustomer);
            }
            else
            {
                currentCustomer = null;
            }
        }
    }
    public void UpdateQueuePositions()
    {
        for (int i = 0; i < customerQueue.Count; i++)
        {
            ClientController customer = customerQueue[i];
            Vector3 newQueuePosition = buyPointTransform.position - new Vector3(2 * i, 0, 0);
            if (i == 0)
            {
                customer.MoveToQueuePosition(newQueuePosition,true);
            }
            else
            {
                customer.MoveToQueuePosition(newQueuePosition, false);
            }
        }
    
    }
    public void CancelService()
    {
        if (currentCustomer != null)
        {
            currentCustomer = null;
        }
    }

    public void RemoveClientFromQueue(ClientController client)
    {
        if (customerQueue.Contains(client))
        {
            int clientIndex = customerQueue.IndexOf(client);
            customerQueue.RemoveAt(clientIndex);

            for (int i = clientIndex; i < customerQueue.Count; i++)
            {
                Vector3 newQueuePosition = buyPointTransform.position - new Vector3(2 * (i+1), 0, 0);

                customerQueue[i].MoveToQueuePosition(newQueuePosition, false);
            }
        }
    }

}

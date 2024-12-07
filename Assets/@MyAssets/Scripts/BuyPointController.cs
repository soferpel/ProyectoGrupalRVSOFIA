using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuyPointController : MonoBehaviour
{
    public Transform buyPointTransform;
    public List<PersonController> customerQueue = new List<PersonController>();
    public int maxQueueSize = 5;
    public PersonController currentCustomer = null;

    public bool IsOccupied()
    {
        return currentCustomer != null;
    }

    public bool PlaceInQueue(PersonController customer)
    {
        if (customer.TryGetComponent(out MafiaController _))
        {
            customerQueue.Add(customer);
            customer.inQueue = true;
            return true;
        }
        else
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
    }

    public void OccupyPoint(PersonController customer)
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
                PersonController nextCustomer = customerQueue[0];
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
            PersonController customer = customerQueue[i];
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

    public void RemoveClientFromQueue(PersonController client)
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

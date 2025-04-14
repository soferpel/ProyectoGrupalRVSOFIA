using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuyPointControllerMP : MonoBehaviour
{
    public Transform buyPointTransform;
    public List<PersonControllerMP> customerQueue = new List<PersonControllerMP>();
    public int maxQueueSize = 5;
    public PersonControllerMP currentCustomer = null;

    public bool IsOccupied()
    {
        return currentCustomer != null;
    }

    public bool PlaceInQueue(PersonControllerMP customer)
    {
        if (customer.TryGetComponent(out MafiaControllerMP _))
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

    public void OccupyPoint(PersonControllerMP customer)
    {
        customer.inBuyPoint = true;
        currentCustomer = customer;
    }

    public void FreePoint()
    {
        if (currentCustomer != null)
        {
            currentCustomer.inBuyPoint = false;
            UpdateQueuePositions();
            Debug.Log("x actualizada lista");
            if (customerQueue.Count > 0)
            {
                PersonControllerMP nextCustomer = customerQueue[0];
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
            PersonControllerMP customer = customerQueue[i];
            Vector3 newQueuePosition = buyPointTransform.position - new Vector3(2 * i, 0, 0);
            if (i == 0)
            {
                customer.MoveToQueuePosition(newQueuePosition, true);
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

    public void RemoveClientFromQueue(PersonControllerMP client)
    {
        client.inQueue = false;
        if (customerQueue.Contains(client))
        {
            int clientIndex = customerQueue.IndexOf(client);
            customerQueue.RemoveAt(clientIndex);

            for (int i = clientIndex; i < customerQueue.Count; i++)
            {
                Vector3 newQueuePosition = buyPointTransform.position - new Vector3(2 * (i + 1), 0, 0);

                customerQueue[i].MoveToQueuePosition(newQueuePosition, false);
            }
        }
    }

}

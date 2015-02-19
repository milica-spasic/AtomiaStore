﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Atomia.Store.PublicBillingApi;
using Atomia.Store.PublicBillingApi.Handlers;
using Atomia.Store.Core;
using Atomia.Web.Plugin.OrderServiceReferences.AtomiaBillingPublicService;

namespace Atomia.Store.PublicOrderHandlers
{
    public class CampaignCodeHandler : OrderDataHandler
    {
        public override PublicOrder AmendOrder(PublicOrder order, PublicOrderContext orderContext)
        {
            if (!String.IsNullOrEmpty(orderContext.Cart.CampaignCode))
            {
                Add(order, new PublicOrderCustomData
                {
                    Name = "CampaignCode",
                    Value = orderContext.Cart.CampaignCode
                });
            }

            return order;
        }
    }
}

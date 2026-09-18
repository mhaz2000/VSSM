namespace VSSM.Presentation.Web.Models.Base;

public static class MenuConfig
{
    public static List<MenuItem> GetMenu()
    {
        return new List<MenuItem>
        {
            // ---------------- Dashboard ----------------
            new MenuItem
            {
                Title = "داشبورد",
                Icon = "icon-speedometer",
                Controller = "Home",
                Action = "Index"
            },

            // ---------------- Base Info ----------------
            new MenuItem
            {
                Icon = "fa fa-file",
                Title = "اطلاعات پایه",
                Children =
                {
                    // کالاها
                    new MenuItem
                    {
                        Title = "کالاها",
                        Children =
                        {
                            new MenuItem { Title = "نوع کالا", Controller = "ItemClass", Action = "Index" },
                            new MenuItem { Title = "کالاها", Controller = "Item", Action = "Index" },
                            new MenuItem { Title = "دوز بر ویال", Controller = "DosePerVial", Action = "Index" },
                            new MenuItem { Title = "دوز اختصاص داده شده به کالا", Controller = "ItemDoseAssignment", Action = "Index" }
                        }
                    },

                    new MenuItem { Title = "سطح", Controller = "Level", Action = "Index" },
                    new MenuItem { Title = "دلایل تعدیل", Controller = "AdjustReason", Action = "Index" },

                    // اطلاعات مربوط به برنامه
                    new MenuItem
                    {
                        Title = "اطلاعات مربوط به برنامه",
                        Children =
                        {
                            new MenuItem { Title = "روش دریافت", Controller = "ReceivingMode", Action = "Index" },
                            new MenuItem { Title = "حالات انتقال", Controller = "TransportMode", Action = "Index" },
                            new MenuItem { Title = "شرایط اقلیمی", Controller = "Climate", Action = "Index" },
                            new MenuItem { Title = "نوع راه", Controller = "RoadType", Action = "Index" },
                            new MenuItem { Title = "نوع مالکیت", Controller = "Ownership", Action = "Index" }
                        }
                    },

                    // سازندگان
                    new MenuItem
                    {
                        Title = "سازندگان",
                        Children =
                        {
                            new MenuItem { Title = "سازندگان", Controller = "Manufacturer", Action = "Index" },
                            new MenuItem { Title = "نامهای تجاری", Controller = "CommercialName", Action = "Index" }
                        }
                    },

                    // تجهیزات
                    new MenuItem
                    {
                        Title = "تجهیزات",
                        Children =
                        {
                            new MenuItem { Title = "نوع انبار", Controller = "StoreType", Action = "Index" },
                            new MenuItem { Title = "منبع انرژی", Controller = "PowerSource", Action = "Index" }
                        }
                    },

                    // VVM
                    new MenuItem
                    {
                        Title = "VVM",
                        Children =
                        {
                            new MenuItem { Title = "مرحله VVM", Controller = "Vvmstage", Action = "Index" },
                            new MenuItem { Title = "انواع VVM", Controller = "Vvmtype", Action = "Index" }
                        }
                    },

                    // اطلاعات کاربران
                    new MenuItem
                    {
                        Title = "اطلاعات مربوط به کاربران",
                        Children =
                        {
                            new MenuItem { Title = "تحصیلات", Controller = "EduType", Action = "Index" },
                            new MenuItem { Title = "عنوان شغل", Controller = "JobType", Action = "Index" }
                        }
                    },

                    new MenuItem { Title = "انتقال گیرنده", Controller = "Home", Action = "UnderConstruction" },
                    new MenuItem { Title = "کمک", Controller = "Home", Action = "UnderConstruction" },
                    new MenuItem { Title = "انبار", Controller = "Store", Action = "Index" }
                }
            },

            // ---------------- User Management ----------------
            new MenuItem
            {
                Icon = "fa fa-users",
                Title = "مدیریت کاربران",
                Children =
                {
                    new MenuItem { Title = "کاربران", Controller = "User", Action = "Index" },
                    new MenuItem { Title = "نقشها", Controller = "Role", Action = "Index" },
                    //new MenuItem { Title = "منوها", Controller = "Home", Action = "UnderConstruction" },
                    //new MenuItem { Title = "دسترسی نقش به منوها", Controller = "Home", Action = "UnderConstruction" },
                    new MenuItem { Title = "ورود در نقش مدیر سیستم", Controller = "Home", Action = "UnderConstruction" },
                    new MenuItem { Title = "مدیریت محل خدمت کاربران", Controller = "UserManagement", Action = "ManageRecipient" },
                }
            },

            // ---------------- Base Info 2 ----------------
            new MenuItem
            {
                Icon = "fa fa-clone",
                Title = "اطلاعات پایه 2",
                Children =
                {
                    //new MenuItem { Title = "تنظیمات", Controller = "RecipientConfiguration", Action = "Index" },
                    new MenuItem { Title = "گیرندگان", Controller = "Recipient", Action = "Index" },
                    new MenuItem { Title = "مقدار حداقل و حداثر برای کالاها", Controller = "MinMaxItem", Action = "Index" }
                }
            },

            // ---------------- Arrivals ----------------
            new MenuItem
            {
                Icon = "fa fa-ambulance",
                Title = "ورودی ها",
                Children =
                {
                    new MenuItem { Title = "مدیریت ورودی", Controller = "Arrival", Action = "Index" },
                    new MenuItem { Title = "مشاهده محموله‌های رسیده", Controller = "Dispatches", Action = "ToBeArrivedIndex" },
                    new MenuItem { Title = "موجودی اولیه", Controller = "Home", Action = "UnderConstruction" },
                    new MenuItem { Title = "انتقال کالا بین انبار", Controller = "StoreTransfer", Action = "Transfer" }
                }
            },

            // ---------------- General Settings ----------------
            new MenuItem
            {
                Icon = "fa fa-gear",
                Title = "تنظیمات کلی",
                Children =
                {
                    new MenuItem { Title = "Arrival report config", Controller = "ArrivalRepConfig", Action = "Index" },
                    new MenuItem { Title = "Dispatch report config", Controller = "DispatchRepConfig", Action = "Index" },
                    new MenuItem { Title = "تنظیمات فاکتور", Controller = "VoucherConfig", Action = "Index" },
                    new MenuItem { Title = "فرهنگ ها", Controller = "Home", Action = "UnderConstruction" },
                    new MenuItem { Title = "دوره های فعالیت", Controller = "FiscalPeriod", Action = "Index" },
                    new MenuItem { Title = "مدیریت صفحه نخست کاربر", Controller = "Home", Action = "UnderConstruction" },
                    new MenuItem { Title = "اخطار میزان مصرف انبار", Controller = "StoreUsageWarning", Action = "Index" },
                    new MenuItem { Title = "مدیریت لوگوها" },
                    new MenuItem { Title = "تازه ها" }
                }
            },

            // ---------------- Dispatch ----------------
            new MenuItem
            {
                Icon = "fa fa-plane",
                Title = "ارسالات",
                Controller = "Dispatches",
                Action = "Index"
            },

            // ---------------- Adjustment ----------------
            new MenuItem
            {
                Icon = "fa fa-user-times",
                Title = "تعدیل",
                Children =
                {
                    new MenuItem { Title = "تعدیل مقدار جاری", Controller = "Adjustment", Action = "Index" },
                    new MenuItem { Title = "تعدیل ارسالات", Controller = "Home", Action = "UnderConstruction" },
                    new MenuItem { Title = "تعدیل ورودی ها", Controller = "Home", Action = "UnderConstruction" },
                    new MenuItem { Title = "تعدیل محموله های رسیده", Controller = "Home", Action = "UnderConstruction" }
                }
            },

            // ---------------- Request ----------------
            new MenuItem
            {
                Icon = "fa fa-envelope-o",
                Title = "درخواست",
                Children =
                {
                    new MenuItem { Title = "مدیریت درخواست ها", Controller = "Request", Action = "Index" },
                    new MenuItem { Title = "درخواست های رسیده", Controller = "Request", Action = "ReceivedRequests" }
                }
            },

            // ---------------- Warnings ----------------
            new MenuItem
            {
                Icon = "fa fa-warning",
                Title = "اخطار‌ها",
                Children =
                {
                    new MenuItem { Title = "کالا‌های بالای حداکثر", Controller = "Warnings", Action = "HighItemsMaxIndex" },
                    new MenuItem { Title = "کالا‌های زیر حداقل", Controller = "Warnings", Action = "LowItemsMinIndex" },
                    new MenuItem { Title = "کالاهای نزدیک به انقضا", Controller = "Warnings", Action = "NearExpirationItemsIndex" },
                    new MenuItem { Title = "امحا UID", Controller = "DestroyUid", Action = "Index" }
                }
            },
             new MenuItem
            {
                Icon = "fa fa-chart-bar",
                Title = "گزارشات",
                Children =
                {
                    new MenuItem
                    {
                        Title = "گزارش‌های موجودی",
                        Children =
                        {
                            new MenuItem
                            {
                                Title = "مقدار موجود",
                                Controller = "CurrentStockReport",
                                Action = "AvailableQuantity",
                            },
                            new MenuItem
                            {
                                Title = "گزارش ورود و خروج",
                                Controller = "EntryExitReport",
                                Action = "Index",
                            },
                        }
                    },
                }
            },



            new MenuItem
            {
                Icon = "fa fa-history",
                Title = "تاریخچه واکسن",
                Controller = "UidHistory",
                Action = "Index"
            },

            // ---------------- Logout ----------------
            new MenuItem
            {
                Icon = "fa fa-sign-out",
                Title = "خروج",
                Controller = "Account",
                Action = "Logout"
            }
        };
    }
}

﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TelerikMvcWebMail.Models
{
    public class ContactsService
    {
        private WebMailEntities entities;

        private static bool UpdateDatabase = false;

        public ContactsService(WebMailEntities entities)
        {
            this.entities = entities;
        }

        public IList<ContactViewModel> GetAll()
        {
            IList<ContactViewModel> result = HttpContext.Current.Session["Contacts"] as IList<ContactViewModel>;

            if (result == null || UpdateDatabase)
            {
                result = entities.Contacts.Select(e => new ContactViewModel
                {
                    EmployeeID = e.Id,
                    Name = e.Name,
                    HomePhone = e.Phone,
                    Country = e.Country,
                    City = e.City,
                    Company = e.Company,
                    Folder = e.Folder,
                    Title = e.Title,
                    Email = e.Email
                }).ToList();

                if (!UpdateDatabase)
                {
                    HttpContext.Current.Session["Contacts"] = result;
                }
            }

            return result;
        }

        public IEnumerable<ContactViewModel> Read()
        {
            return GetAll();
        }

        public void Insert(ContactViewModel contact)
        {
            if (string.IsNullOrEmpty(contact.Name))
            {
                contact.Name = "New contact";
            }

            if (!UpdateDatabase)
            {
                GetAll().Insert(0, contact);
            }
            else
            {
                var entity = contact.ToEntity();

                entities.Contacts.Add(entity);
                entities.SaveChanges();

                contact.EmployeeID = entity.Id;
            }
        }

        public void Update(ContactViewModel contact)
        {
            if (string.IsNullOrEmpty(contact.Name))
            {
                contact.Name = "";
            }

            if (!UpdateDatabase)
            {
                var target = One(e => e.EmployeeID == contact.EmployeeID);

                if (target != null)
                {
                    target.Folder = contact.Folder;
                    target.City = contact.City;
                    target.Company = contact.Company;
                    target.Country = contact.Country;
                    target.Email = contact.Email;
                    target.HomePhone = contact.HomePhone;
                    target.Name = contact.Name;
                    target.Title = contact.Title;
                }
            }
            else
            {
                var entity = contact.ToEntity();
                entities.Contacts.Attach(entity);
                entities.Entry(entity).State = EntityState.Modified;
                entities.SaveChanges();
            }
        }

        public void Delete(ContactViewModel contact)
        {
            if (!UpdateDatabase)
            {
                var target = One(p => p.EmployeeID == contact.EmployeeID);
                if (target != null)
                {
                    GetAll().Remove(target);
                }
            }
            else
            {
                var entity = contact.ToEntity();
                entities.Contacts.Attach(entity);
                entities.Contacts.Remove(entity);
                entities.SaveChanges();
            }
        }

        public ContactViewModel One(Func<ContactViewModel, bool> predicate)
        {
            return GetAll().FirstOrDefault(predicate);
        }

        public void Dispose()
        {
            entities.Dispose();
        }
    }
}
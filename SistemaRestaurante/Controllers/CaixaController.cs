﻿using SistemaRestaurante.DAO;
using SistemaRestaurante.Filters;
using SistemaRestaurante.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaRestaurante.Controllers
{
    [AutorizacacoFilter]
    public class CaixaController : Controller
    {
        [Route("Caixa", Name = "ViewCaixa")]
        public ActionResult Index()
        {
            Usuario user = (Usuario)Session["Admin"];
            if (user.Cargo.Equals("CAIXA") || user.Cargo.Equals("GERENTE"))
            {
                return View();
            }
            else
            {
                return RedirectToRoute("Sair");
            }
        }

        [Route("BuscaPedido")]
        public ActionResult BuscaPedido(int numeroComanda)
        {
            ComandaDAO comandaDAO = new ComandaDAO();
            PedidoDAO pedidoDao = new PedidoDAO();
            ItemPedidoDAO itemDao = new ItemPedidoDAO();
            Comanda comanda = comandaDAO.BuscaPorNumero(numeroComanda);
            if (comanda == null)
            {
                return Json(new { success = false, resposta = "Comanda não existe" }, JsonRequestBehavior.AllowGet);
            }else if (comanda.MesaId == null)
            {
                return Json(new { success = false, resposta = "Comanda não está vinculada a uma mesa" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                Pedido pedido = pedidoDao.BuscaPorComanda(comanda.Id);
                IList<ItemPedido> itens = itemDao.ListarPorPedido(pedido.Id);
                if (itens.Count == 0)
                {
                    return Json(new { success = false, resposta = "Comanda não possui pedidos" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = true, ItemPedido = itens, Total = pedido.ValorTotal, resposta = "Pedidos carregados com sucesso" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [Route("CarregaCartao")]
        public ActionResult CarregaCartao(int numeroCartao,string total)
        {
            CartaoDAO dao = new CartaoDAO();
            Cartao cartao = dao.BuscaPorNumero(numeroCartao);
            if (cartao == null)
            {
                return Json(new { success = false, resposta = "Cartão não existe" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                double valor = Convert.ToDouble(total);
                double desconto = (valor * 0.10)/100;
                return Json(new { success = true, Total = String.Format("{0:0.00}", desconto).Replace(",","."),resposta = "Desconto aplicado com sucesso"}, JsonRequestBehavior.AllowGet);
            }
        }

        [Route("FinalizaComanda")]
        public ActionResult FinalizaComanda(int nmrComanda)
        {
            ComandaDAO dao = new ComandaDAO();
            PedidoDAO pedDao = new PedidoDAO();
            Comanda comanda = dao.BuscaPorNumero(nmrComanda);
            if (comanda == null)
            {
                return Json(new { success = false, resposta = "Comanda nao existe" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                Pedido pedido = pedDao.BuscaPorComanda(comanda.Id);
                comanda.MesaId = null;
                pedido.ComandaId = null;
                dao.Atualizar(comanda);
                pedDao.Atualizar(pedido);
                return Json(new { success = true, resposta = "Comanda finalizada com sucesso" }, JsonRequestBehavior.AllowGet);
            }
        }

    }



}
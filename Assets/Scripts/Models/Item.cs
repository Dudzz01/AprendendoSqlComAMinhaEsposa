using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Models
{

    public class Item
    {
        public int IdItem { get; set; }
        public int IdMovel { get; set; }
        public string NomeItem { get; set; }
        public string Descricao { get; set; }

        public string Dica { get; set; }

        public string Tipo { get; set; } = "ESTOQUE";   
        public int Quantidade { get; set; } = 0;
        public string Unidade { get; set; } = "un";
        public string ValidoAte { get; set; }          

        public string Status { get; set; } = "OK";     
        public int Prioridade { get; set; } = 3;
    }

}

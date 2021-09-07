﻿using Saiive.SuperNode.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saiive.SuperNode.Abstaction.Providers
{
    public interface ITokenProvider
    {
        Task<TokenModel> GetToken(string network, string tokenName);
        Task<IList<TokenModel>> GetAll(string network);
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace RoslynTool.CsToDsl
{
    internal class ReturnContinueBreakAnalysis : CSharpSyntaxWalker
    {
        public bool Exist
        {
            get { return m_ExistReturn || m_ExistContinue || m_ExistBreak; }
        }
        public bool ExistReturn
        {
            get { return m_ExistReturn; }
        }
        public bool ExistContinue
        {
            get { return m_ExistContinue; }
        }
        public bool ExistBreak
        {
            get { return m_ExistBreak; }
        }
        public override void VisitReturnStatement(ReturnStatementSyntax node)
        {
            m_ExistReturn = true;
        }
        public override void VisitContinueStatement(ContinueStatementSyntax node)
        {
            if (m_InLoop <= 0) {
                m_ExistContinue = true;
            }
        }
        public override void VisitBreakStatement(BreakStatementSyntax node)
        {
            if (m_InLoop <= 0 && m_InSwitch <= 0) {
                m_ExistBreak = true;
            }
        }
        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            ++m_InLoop;
            base.VisitWhileStatement(node);
            --m_InLoop;
        }
        public override void VisitDoStatement(DoStatementSyntax node)
        {
            ++m_InLoop;
            base.VisitDoStatement(node);
            --m_InLoop;
        }
        public override void VisitForStatement(ForStatementSyntax node)
        {
            ++m_InLoop;
            base.VisitForStatement(node);
            --m_InLoop;
        }
        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            ++m_InLoop;
            base.VisitForEachStatement(node);
            --m_InLoop;
        }
        public override void VisitSwitchStatement(SwitchStatementSyntax node)
        {
            ++m_InSwitch;
            base.VisitSwitchStatement(node);
            --m_InSwitch;
        }

        private bool m_ExistReturn = false;
        private bool m_ExistContinue = false;
        private bool m_ExistBreak = false;
        private int m_InLoop = 0;
        private int m_InSwitch = 0;
    }
}

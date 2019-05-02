using System;
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
            if (!m_IsInLoop) {
                m_ExistContinue = true;
            }
        }
        public override void VisitBreakStatement(BreakStatementSyntax node)
        {
            if (!m_IsInLoop && !m_IsInSwitch) {
                m_ExistBreak = true;
            }
        }
        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            m_IsInLoop = true;
            base.VisitWhileStatement(node);
            m_IsInLoop = false;
        }
        public override void VisitDoStatement(DoStatementSyntax node)
        {
            m_IsInLoop = true;
            base.VisitDoStatement(node);
            m_IsInLoop = false;
        }
        public override void VisitForStatement(ForStatementSyntax node)
        {
            m_IsInLoop = true;
            base.VisitForStatement(node);
            m_IsInLoop = false;
        }
        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            m_IsInLoop = true;
            base.VisitForEachStatement(node);
            m_IsInLoop = false;
        }
        public override void VisitSwitchStatement(SwitchStatementSyntax node)
        {
            m_IsInSwitch = true;
            base.VisitSwitchStatement(node);
            m_IsInSwitch = false;
        }

        private bool m_ExistReturn = false;
        private bool m_ExistContinue = false;
        private bool m_ExistBreak = false;
        private bool m_IsInLoop = false;
        private bool m_IsInSwitch = false;
    }
}
